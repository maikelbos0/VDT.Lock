
export async function load(key) {
    if (!chrome || !chrome.storage?.sync) {
        return null;
    }

    try {
        var storage = await chrome.storage.sync.get(key);

        return Uint8Array.from(atob(storage[key]), c => c.charCodeAt(0));
    } catch (e) {
        return null;
    }
}

export async function clear() {
    if (!chrome || !chrome.storage?.sync) {
        return false;
    }

    try {
        await chrome.storage.sync.clear();

        return true;
    } catch (ex) {
        return false;
    }
}

export async function save(key, encryptedBytes) {
    if (!chrome || !chrome.storage?.sync) {
        return false;
    }

    try {
        await chrome.storage.sync.set({ [key]: btoa(String.fromCharCode(...encryptedBytes)) });

        return true;
    } catch (ex) {
        return false;
    }
}
