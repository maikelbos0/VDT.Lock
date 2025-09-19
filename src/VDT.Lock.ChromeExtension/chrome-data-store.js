
export async function Load() {
    if (!chrome.storage.local) {
        return null;
    }

    try {
        var storage = await chrome.storage.local.get('value');

        return Uint8Array.from(atob(storage.value), c => c.charCodeAt(0));
    } catch (e) {
        return null;
    }
}

export async function Save(encryptedBytes) {
    if (!chrome.storage.local) {
        return false;
    }

    try {
        await chrome.storage.local.set({ value: btoa(String.fromCharCode(...encryptedBytes)) });

        return true;
    } catch (ex) {
        return false;
    } 
}
