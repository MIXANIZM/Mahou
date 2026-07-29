'use strict';

(() => {
  if (globalThis.__MAHOU_EDITING_CORE_CONTENT_SCRIPT_V1__) return;
  globalThis.__MAHOU_EDITING_CORE_CONTENT_SCRIPT_V1__ = true;

  const core = globalThis.MahouEditingCore;
  if (!core) return;

  const expectedMarker = 'AGZ-MAH-0007-DIAGNOSTIC-V1';
  if (core.MARKER !== expectedMarker) return;
  const exactMarkerSelector = 'meta[name="mahou-chrome-smart-caps-diagnostic"][content="' + expectedMarker + '"]';
  const initialDocument = document;
  const documentId = crypto.randomUUID();
  const elementIds = new WeakMap();
  const composing = new WeakSet();
  const seenRequestIds = new Set();

  function elementId(element) {
    if (!element || typeof element !== 'object') return 'none';
    let id = elementIds.get(element);
    if (!id) {
      id = crypto.randomUUID();
      elementIds.set(element, id);
    }
    return id;
  }

  function isHidden(element) {
    if (!element || element.hidden || element.getAttribute('aria-hidden') === 'true') return true;
    const style = getComputedStyle(element);
    return style.display === 'none' || style.visibility === 'hidden' || style.visibility === 'collapse';
  }

  function classify(element) {
    if (!(element instanceof Element)) return { controlKind: 'unknown', protected: true };
    if (element.isContentEditable) return { controlKind: 'contenteditable', protected: true };
    if (element instanceof HTMLInputElement) {
      const inputType = String(element.type || '').toLowerCase();
      if (inputType === 'password') return { controlKind: 'password', protected: true };
      if (inputType === 'text') return { controlKind: 'input-text', protected: false };
      return { controlKind: 'unknown-input-type', protected: true };
    }
    if (element instanceof HTMLTextAreaElement) return { controlKind: 'textarea', protected: false };
    return { controlKind: 'unknown', protected: true };
  }

  function captureState() {
    const element = document.activeElement;
    const kind = classify(element);
    const base = {
      marker: document.querySelector(exactMarkerSelector) ? core.MARKER : null,
      topFrame: window.top === window,
      documentActive: document === initialDocument && document.visibilityState === 'visible',
      documentFocused: document.hasFocus(),
      documentId,
      elementId: elementId(element),
      controlKind: kind.controlKind,
      connected: Boolean(element && element.isConnected),
      focused: Boolean(element && document.activeElement === element),
      compositionActive: Boolean(element && composing.has(element)),
      readOnly: false,
      disabled: false,
      hidden: Boolean(element && isHidden(element)),
      value: null,
      selectionStart: null,
      selectionEnd: null
    };

    if (kind.protected || !(element instanceof HTMLInputElement || element instanceof HTMLTextAreaElement)) return base;
    base.readOnly = element.readOnly === true;
    base.disabled = element.disabled === true;
    base.value = element.value;
    base.selectionStart = element.selectionStart;
    base.selectionEnd = element.selectionEnd;
    return base;
  }

  function render(result) {
    const node = document.getElementById('extension-result');
    if (!node || !document.querySelector(exactMarkerSelector)) return;
    node.textContent = JSON.stringify(result, null, 2);
    node.dataset.status = result.status || 'rejected';
  }

  document.addEventListener('compositionstart', event => {
    if (event.target instanceof Element) composing.add(event.target);
  }, true);
  document.addEventListener('compositionend', event => {
    if (event.target instanceof Element) composing.delete(event.target);
  }, true);

  chrome.runtime.onMessage.addListener((message, sender, sendResponse) => {
    if (!message || message.type !== 'mahou-editing-core-probe-v1' || !Array.isArray(message.requests)) return false;

    (async () => {
      if (!document.querySelector(exactMarkerSelector)) {
        const result = { status: 'rejected', reason: 'unknown-page' };
        sendResponse(result);
        return;
      }

      const attempts = [];
      for (const request of message.requests) {
        const before = captureState();
        const prepared = core.prepare(before, request, Date.now(), seenRequestIds);
        if (!prepared.ok) {
          attempts.push({ requestId: request.requestId, reason: prepared.reason });
          continue;
        }

        await new Promise(resolve => setTimeout(resolve, 75));
        const current = captureState();
        const revalidated = core.revalidate(prepared.value, current, request, Date.now());
        if (!revalidated.ok) {
          const result = { status: 'rejected', reason: revalidated.reason, attempts };
          render(result);
          sendResponse(result);
          return;
        }

        const capability = core.assessMutationCapability();
        const result = {
          status: capability.status,
          reason: capability.reason,
          requestId: request.requestId,
          control: current.controlKind,
          targetStart: prepared.value.target.targetStart,
          targetEnd: prepared.value.target.targetEnd,
          caret: prepared.value.selectionStart,
          attempts,
          note: 'No text mutation was attempted.'
        };
        render(result);
        sendResponse(result);
        return;
      }

      const result = { status: 'rejected', reason: 'no-fixed-candidate-match', attempts };
      render(result);
      sendResponse(result);
    })().catch(error => {
      const result = { status: 'rejected', reason: 'internal-fail-closed', detail: String(error && error.message ? error.message : error) };
      render(result);
      sendResponse(result);
    });

    return true;
  });
})();
