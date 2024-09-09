
export async function Encrypt(input, keyData, iv) {
    var key = await crypto.subtle.importKey(
        "raw",
        keyData.buffer,
        //{ name: "HMAC", hash: "SHA-256" },
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
        input,
    ));
}
