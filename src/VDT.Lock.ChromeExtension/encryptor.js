
export async function Encrypt(plainBytes, key, iv) {
    var key = await crypto.subtle.importKey(
        "raw",
        key.buffer,
        "AES-CBC",
        false,
        ["encrypt"] //, "decrypt"
    );

    return new Uint8Array(await window.crypto.subtle.encrypt(
        {
            name: "AES-CBC",
            iv: iv,
        },
        key,
        plainBytes,
    ));
}

export async function Decrypt(encryptedBytes, key, iv) {
    var key = await crypto.subtle.importKey(
        "raw",
        key.buffer,
        "AES-CBC",
        false,
        ["decrypt"]
    );

    return new Uint8Array(await window.crypto.subtle.decrypt(
        {
            name: "AES-CBC",
            iv: iv
        },
        key,
        encryptedBytes
    ));
}
