
export async function Load(sectionName) {
    if (!chrome || !chrome.storage || !chrome.storage.sync) {
        return null;
    }

    try {
        var storage = await chrome.storage.sync.get(sectionName);

        return Uint8Array.from(atob(storage.value), c => c.charCodeAt(0));
    } catch (e) {
        return null;
    }
}

export async function Clear() {
    if (!chrome || !chrome.storage || !chrome.storage.sync) {
        return false;
    }

    try {
        await chrome.storage.sync.clear();

        return true;
    } catch (ex) {
        return false;
    }
}

export async function Save(sectionName, encryptedBytes) {
    if (!chrome || !chrome.storage || !chrome.storage.sync) {
        return false;
    }

    try {
        await chrome.storage.sync.set({ [sectionName]: btoa(String.fromCharCode(...encryptedBytes)) });

        return true;
    } catch (ex) {
        return false;
    }
}
