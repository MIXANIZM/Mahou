'use strict';

const assert = require('assert');
const core = require('../../integrations/chrome-smart-caps/prototype-extension/editing-core.js');

let requestSequence = 0;
function request(overrides = {}) {
  requestSequence += 1;
  return {
    version: 1,
    requestId: 'request-' + String(requestSequence).padStart(4, '0'),
    createdAt: 1000,
    expectedWord: 'ПРИвет',
    replacement: 'Привет',
    maxAgeMs: 2500,
    ...overrides
  };
}

function snapshot(overrides = {}) {
  const value = overrides.value === undefined ? 'LEFT ПРИвет RIGHT' : overrides.value;
  const caret = overrides.selectionStart === undefined ? 'LEFT ПРИвет '.length : overrides.selectionStart;
  return {
    marker: core.MARKER,
    topFrame: true,
    documentActive: true,
    documentFocused: true,
    documentId: 'document-1',
    elementId: 'element-1',
    controlKind: 'input-text',
    connected: true,
    focused: true,
    compositionActive: false,
    readOnly: false,
    disabled: false,
    hidden: false,
    value,
    selectionStart: caret,
    selectionEnd: overrides.selectionEnd === undefined ? caret : overrides.selectionEnd,
    ...overrides
  };
}

function expectReject(result, reason) {
  assert.strictEqual(result.ok, false);
  assert.strictEqual(result.reason, reason);
}

{
  const seen = new Set();
  const req = request();
  const result = core.prepare(snapshot(), req, 1100, seen);
  assert.strictEqual(result.ok, true);
  assert.strictEqual(result.value.target.targetStart, 5);
  assert.strictEqual(result.value.target.targetEnd, 11);
  assert.strictEqual(result.value.target.prefix, 'LEFT ');
  assert.strictEqual(result.value.target.suffix, ' RIGHT');
}

{
  const seen = new Set();
  const req = request();
  assert.strictEqual(core.prepare(snapshot({ controlKind: 'textarea' }), req, 1100, seen).ok, true);
}

{
  const cases = [
    [snapshot({ marker: 'wrong' }), 'unknown-page'],
    [snapshot({ topFrame: false }), 'wrong-frame'],
    [snapshot({ documentActive: false }), 'stale-document'],
    [snapshot({ focused: false }), 'focus-changed'],
    [snapshot({ connected: false }), 'detached-control'],
    [snapshot({ controlKind: 'contenteditable', value: null, selectionStart: null, selectionEnd: null }), 'contenteditable-control'],
    [snapshot({ controlKind: 'password', value: null, selectionStart: null, selectionEnd: null }), 'password-control'],
    [snapshot({ controlKind: 'unknown-input-type', value: null, selectionStart: null, selectionEnd: null }), 'unsupported-control'],
    [snapshot({ readOnly: true }), 'readonly-control'],
    [snapshot({ disabled: true }), 'disabled-control'],
    [snapshot({ hidden: true }), 'hidden-control'],
    [snapshot({ compositionActive: true }), 'composition-active'],
    [snapshot({ selectionStart: 6, selectionEnd: 9 }), 'non-collapsed-selection']
  ];
  for (const [state, reason] of cases) {
    expectReject(core.prepare(state, request(), 1100, new Set()), reason);
  }
}

{
  expectReject(core.prepare(snapshot(), request({ createdAt: 0 }), 3000, new Set()), 'stale-request');
  const seen = new Set();
  const duplicate = request();
  assert.strictEqual(core.prepare(snapshot(), duplicate, 1100, seen).ok, true);
  expectReject(core.prepare(snapshot(), duplicate, 1100, seen), 'duplicate-request');
}

{
  expectReject(core.prepare(snapshot({ value: 'LEFT xПРИвет RIGHT', selectionStart: 'LEFT xПРИвет '.length, selectionEnd: 'LEFT xПРИвет '.length }), request(), 1100, new Set()), 'target-boundary-mismatch');
  expectReject(core.prepare(snapshot({ value: 'LEFT ПРИветRIGHT', selectionStart: 11, selectionEnd: 11 }), request(), 1100, new Set()), 'delimiter-missing');
}

{
  const req = request();
  const before = snapshot();
  const prepared = core.prepare(before, req, 1100, new Set());
  assert.strictEqual(prepared.ok, true);
  expectReject(core.revalidate(prepared.value, snapshot({ documentId: 'document-2' }), req, 1200), 'stale-document');
  expectReject(core.revalidate(prepared.value, snapshot({ elementId: 'element-2' }), req, 1200), 'stale-element');
  expectReject(core.revalidate(prepared.value, snapshot({ value: 'LEFT ПРИвет! RIGHT', selectionStart: 12, selectionEnd: 12 }), req, 1200), 'stale-value');
  expectReject(core.revalidate(prepared.value, snapshot({ selectionStart: 11, selectionEnd: 11 }), req, 1200), 'stale-caret');
  expectReject(core.revalidate(prepared.value, snapshot({ compositionActive: true }), req, 1200), 'composition-active');
  expectReject(core.revalidate(prepared.value, before, req, 4000), 'stale-request');
  assert.strictEqual(core.revalidate(prepared.value, before, req, 1200).ok, true);
}

{
  const req = request();
  const before = snapshot();
  const prepared = core.prepare(before, req, 1100, new Set());
  const after = snapshot({
    value: 'LEFT Привет RIGHT',
    selectionStart: 'LEFT Привет '.length,
    selectionEnd: 'LEFT Привет '.length
  });
  assert.strictEqual(core.verifyPostMutation(prepared.value, after, req.replacement).ok, true);
  expectReject(core.verifyPostMutation(prepared.value, snapshot({ value: 'CHANGED Привет RIGHT' }), req.replacement), 'post-mutation-mismatch');
  expectReject(core.verifyPostMutation(prepared.value, snapshot({ value: 'LEFT Привет CHANGED' }), req.replacement), 'post-mutation-mismatch');
  expectReject(core.verifyPostMutation(prepared.value, snapshot({ value: 'LEFT Привет RIGHT', selectionStart: 5, selectionEnd: 5 }), req.replacement), 'post-mutation-caret-mismatch');
}

{
  const capability = core.assessMutationCapability();
  assert.strictEqual(capability.ok, false);
  assert.strictEqual(capability.status, 'BROWSER-CONTEXT-MUTATION-NOT-SAFE');
  assert.strictEqual(capability.reason, 'mutation-api-not-accepted');
  assert.deepStrictEqual(capability.rejectedMethods.map(item => item.api), [
    'setRangeText',
    'execCommand-insertText',
    'value-setter',
    'synthetic-input-events'
  ]);
}

console.log('Chrome extension editing-core regression passed.');
