(function (root, factory) {
  'use strict';
  const api = factory();
  if (typeof module === 'object' && module.exports) {
    module.exports = api;
  } else {
    root.MahouEditingCore = api;
  }
}(typeof globalThis !== 'undefined' ? globalThis : this, function () {
  'use strict';

  const MARKER = 'AGZ-MAH-0007-DIAGNOSTIC-V1';
  const REQUEST_VERSION = 1;
  const MAX_BOUNDARY_CHARACTERS = 4;
  const FINAL_STATUS = 'BROWSER-CONTEXT-MUTATION-NOT-SAFE';

  function reject(reason, details) {
    return Object.freeze({
      ok: false,
      status: 'rejected',
      reason,
      details: details || null
    });
  }

  function accept(value) {
    return Object.freeze({ ok: true, value });
  }

  function isFiniteInteger(value) {
    return Number.isInteger(value) && Number.isFinite(value);
  }

  function isBoundaryCharacter(value) {
    return typeof value === 'string' && value.length === 1 && /[\s\p{P}\p{S}]/u.test(value);
  }

  function isCoreWordCharacter(value) {
    return typeof value === 'string' && value.length === 1 && /[\p{L}\p{M}\p{N}_]/u.test(value);
  }

  function validateRequest(request, now, seenRequestIds) {
    if (!request || typeof request !== 'object') return reject('invalid-request');
    if (request.version !== REQUEST_VERSION) return reject('unsupported-request-version');
    if (typeof request.requestId !== 'string' || request.requestId.length < 8 || request.requestId.length > 128)
      return reject('invalid-request-id');
    if (!isFiniteInteger(request.createdAt) || !isFiniteInteger(request.maxAgeMs) || request.maxAgeMs < 1 || request.maxAgeMs > 2500)
      return reject('invalid-request-age');
    if (typeof request.expectedWord !== 'string' || request.expectedWord.length < 1 || request.expectedWord.length > 64)
      return reject('invalid-expected-word');
    if (typeof request.replacement !== 'string' || request.replacement.length < 1 || request.replacement.length > 64)
      return reject('invalid-replacement');
    if (request.expectedWord === request.replacement) return reject('no-op-request');

    const age = now - request.createdAt;
    if (!Number.isFinite(age) || age < 0 || age > request.maxAgeMs) return reject('stale-request');
    if (seenRequestIds && seenRequestIds.has(request.requestId)) return reject('duplicate-request');
    if (seenRequestIds) seenRequestIds.add(request.requestId);
    return accept(request);
  }

  function validateSnapshot(snapshot) {
    if (!snapshot || typeof snapshot !== 'object') return reject('invalid-snapshot');
    if (snapshot.marker !== MARKER) return reject('unknown-page');
    if (snapshot.topFrame !== true) return reject('wrong-frame');
    if (snapshot.documentActive !== true || snapshot.documentFocused !== true) return reject('stale-document');
    if (snapshot.connected !== true) return reject('detached-control');
    if (snapshot.focused !== true) return reject('focus-changed');
    if (snapshot.compositionActive === true) return reject('composition-active');

    if (snapshot.controlKind === 'contenteditable') return reject('contenteditable-control');
    if (snapshot.controlKind === 'password') return reject('password-control');
    if (snapshot.controlKind !== 'input-text' && snapshot.controlKind !== 'textarea')
      return reject('unsupported-control');
    if (snapshot.readOnly === true) return reject('readonly-control');
    if (snapshot.disabled === true) return reject('disabled-control');
    if (snapshot.hidden === true) return reject('hidden-control');
    if (typeof snapshot.value !== 'string') return reject('unreadable-source-value');
    if (!isFiniteInteger(snapshot.selectionStart) || !isFiniteInteger(snapshot.selectionEnd))
      return reject('unreadable-caret');
    if (snapshot.selectionStart !== snapshot.selectionEnd) return reject('non-collapsed-selection');
    if (snapshot.selectionStart < 0 || snapshot.selectionStart > snapshot.value.length)
      return reject('invalid-caret');
    return accept(snapshot);
  }

  function findTarget(value, caret, expectedWord) {
    if (typeof value !== 'string' || !isFiniteInteger(caret) || caret < 0 || caret > value.length)
      return reject('target-boundary-mismatch');

    let delimiterLength = 0;
    let cursor = caret;
    while (cursor > 0 && delimiterLength < MAX_BOUNDARY_CHARACTERS && isBoundaryCharacter(value[cursor - 1])) {
      cursor -= 1;
      delimiterLength += 1;
    }
    if (delimiterLength < 1) return reject('delimiter-missing');

    const targetEnd = cursor;
    const targetStart = targetEnd - expectedWord.length;
    if (targetStart < 0 || value.slice(targetStart, targetEnd) !== expectedWord)
      return reject('expected-word-mismatch');
    if (targetStart > 0 && isCoreWordCharacter(value[targetStart - 1]))
      return reject('target-boundary-mismatch');
    if (targetEnd >= value.length || !isBoundaryCharacter(value[targetEnd]))
      return reject('target-boundary-mismatch');

    return accept(Object.freeze({
      targetStart,
      targetEnd,
      caret,
      delimiterLength,
      prefix: value.slice(0, targetStart),
      suffix: value.slice(targetEnd),
      sourceValue: value
    }));
  }

  function prepare(snapshot, request, now, seenRequestIds) {
    const requestResult = validateRequest(request, now, seenRequestIds);
    if (!requestResult.ok) return requestResult;
    const snapshotResult = validateSnapshot(snapshot);
    if (!snapshotResult.ok) return snapshotResult;
    const targetResult = findTarget(snapshot.value, snapshot.selectionStart, request.expectedWord);
    if (!targetResult.ok) return targetResult;

    return accept(Object.freeze({
      request: Object.freeze({ ...request }),
      marker: snapshot.marker,
      documentId: snapshot.documentId,
      elementId: snapshot.elementId,
      controlKind: snapshot.controlKind,
      value: snapshot.value,
      selectionStart: snapshot.selectionStart,
      selectionEnd: snapshot.selectionEnd,
      target: targetResult.value
    }));
  }

  function revalidate(prepared, currentSnapshot, request, now) {
    if (!prepared || !prepared.request || prepared.request.requestId !== request.requestId)
      return reject('stale-request');
    const age = now - request.createdAt;
    if (!Number.isFinite(age) || age < 0 || age > request.maxAgeMs) return reject('stale-request');

    const currentResult = validateSnapshot(currentSnapshot);
    if (!currentResult.ok) return currentResult;
    if (currentSnapshot.documentId !== prepared.documentId) return reject('stale-document');
    if (currentSnapshot.elementId !== prepared.elementId) return reject('stale-element');
    if (currentSnapshot.value !== prepared.value) return reject('stale-value');
    if (currentSnapshot.selectionStart !== prepared.selectionStart || currentSnapshot.selectionEnd !== prepared.selectionEnd)
      return reject('stale-caret');

    const targetResult = findTarget(currentSnapshot.value, currentSnapshot.selectionStart, request.expectedWord);
    if (!targetResult.ok) return targetResult;
    if (targetResult.value.targetStart !== prepared.target.targetStart || targetResult.value.targetEnd !== prepared.target.targetEnd)
      return reject('target-boundary-mismatch');
    if (targetResult.value.prefix !== prepared.target.prefix || targetResult.value.suffix !== prepared.target.suffix)
      return reject('adjacent-text-mismatch');
    return accept(prepared);
  }

  function verifyPostMutation(prepared, afterSnapshot, replacement) {
    if (!prepared || !prepared.target || !afterSnapshot) return reject('post-mutation-mismatch');
    if (afterSnapshot.documentId !== prepared.documentId) return reject('stale-document');
    if (afterSnapshot.elementId !== prepared.elementId) return reject('stale-element');
    if (afterSnapshot.compositionActive === true) return reject('composition-active');
    if (afterSnapshot.selectionStart !== afterSnapshot.selectionEnd) return reject('non-collapsed-selection');

    const expectedValue = prepared.target.prefix + replacement + prepared.target.suffix;
    const expectedCaret = prepared.selectionStart + replacement.length - prepared.request.expectedWord.length;
    if (afterSnapshot.value !== expectedValue) return reject('post-mutation-mismatch');
    if (!afterSnapshot.value.startsWith(prepared.target.prefix) || !afterSnapshot.value.endsWith(prepared.target.suffix))
      return reject('adjacent-text-mismatch');
    if (afterSnapshot.selectionStart !== expectedCaret || afterSnapshot.selectionEnd !== expectedCaret)
      return reject('post-mutation-caret-mismatch');
    return accept(Object.freeze({ expectedValue, expectedCaret }));
  }

  function assessMutationCapability() {
    return Object.freeze({
      ok: false,
      status: FINAL_STATUS,
      reason: 'mutation-api-not-accepted',
      rejectedMethods: Object.freeze([
        Object.freeze({
          api: 'setRangeText',
          reason: 'Exact range and caret can be expressed, but Chrome does not provide a normal trusted editing-event and single undo/redo transaction contract for script calls.'
        }),
        Object.freeze({
          api: 'execCommand-insertText',
          reason: 'Deprecated and requires a temporary programmatic selection of the target range, which is forbidden.'
        }),
        Object.freeze({
          api: 'value-setter',
          reason: 'Whole-control rewrite; forbidden and not a range mutation.'
        }),
        Object.freeze({
          api: 'synthetic-input-events',
          reason: 'Events remain untrusted and do not create a browser editing transaction or safely synchronize controlled framework state.'
        })
      ])
    });
  }

  return Object.freeze({
    MARKER,
    REQUEST_VERSION,
    FINAL_STATUS,
    validateRequest,
    validateSnapshot,
    findTarget,
    prepare,
    revalidate,
    verifyPostMutation,
    assessMutationCapability
  });
}));
