'use strict';

const TEST_ONLY_CANDIDATES = Object.freeze([
  Object.freeze({ expectedWord: 'окоРОчка', replacement: 'окорочка' }),
  Object.freeze({ expectedWord: 'ПРИвет', replacement: 'Привет' }),
  Object.freeze({ expectedWord: 'КуРиные', replacement: 'Куриные' })
]);

function requestId() {
  return 'agz-mah-0007-' + crypto.randomUUID();
}

function makeRequests(createdAt) {
  return TEST_ONLY_CANDIDATES.map(candidate => Object.freeze({
    version: 1,
    requestId: requestId(),
    createdAt,
    expectedWord: candidate.expectedWord,
    replacement: candidate.replacement,
    maxAgeMs: 2500
  }));
}

chrome.action.onClicked.addListener(async tab => {
  if (!Number.isInteger(tab.id) || tab.active !== true) return;

  try {
    const activeTabs = await chrome.tabs.query({ active: true, currentWindow: true });
    if (activeTabs.length !== 1 || activeTabs[0].id !== tab.id || activeTabs[0].windowId !== tab.windowId) return;

    await chrome.scripting.executeScript({
      target: { tabId: tab.id },
      files: ['editing-core.js', 'content-script.js']
    });

    const response = await chrome.tabs.sendMessage(tab.id, {
      type: 'mahou-editing-core-probe-v1',
      requests: makeRequests(Date.now())
    });
    console.info('AGZ-MAH-0007 diagnostic result:', response);
  } catch (error) {
    console.info('AGZ-MAH-0007 diagnostic refused:', String(error && error.message ? error.message : error));
  }
});
