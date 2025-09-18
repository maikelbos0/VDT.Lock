
export async function Load() {
    var storage = await chrome.storage.local.get('value');
    
    return Uint8Array.from(atob(storage.value), c => c.charCodeAt(0));
}

export async function Save(encryptedBytes) {
    return await chrome.storage.local.set({ value: btoa(String.fromCharCode(...encryptedBytes)) });
}
