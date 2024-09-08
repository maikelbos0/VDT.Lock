
export async function Test(input, key) {
    var cryptoKey = await crypto.subtle.importKey(
        "raw" /* format */,
        key.buffer /* keyData */,
        { name: "HMAC", hash: "SHA-256" } /* algorithm (HmacImportParams) */,
        false /* extractable */,
        ["sign", "verify"] /* keyUsages */);

    return `Called Test with input: ${input}, ${cryptoKey}`;
}

//export async function Encrypt(key, inputText) {
//    // 'key' and 'inputText' are each passed as Uint8Array objects, the JS
//    // equivalent of .NET's byte[]. SubtleCrypto uses ArrayBuffer objects
//    // for the key and the input and output buffers.
//    //
//    // To convert Uint8Array -> ArrayBuffer, use the Uint8Array.buffer property.
//    // To convert ArrayBuffer -> Uint8Array, call the Uint8Array constructor.
//    //
//    // Most of SubtleCrypto's functions - including importKey and sign - are
//    // Promise-based. We await them to unwrap the Promise, similar to how we'd
//    // await a Task-based function in .NET.
//    //
//    // https://developer.mozilla.org/docs/Web/API/SubtleCrypto/importKey

//    // Now that we've imported the key, we can compute the HMACSHA256 digest.
//    //
//    // https://developer.mozilla.org/docs/Web/API/SubtleCrypto/sign

//    var digest = await crypto.subtle.sign(
//        "HMAC" /* algorithm */,
//        cryptoKey /* key */,
//        inputText.buffer /* data */);

//    // 'digest' is typed as ArrayBuffer. We need to convert it back to Uint8Array
//    // so that .NET properly translates it to a byte[].

//    return new Uint8Array(digest);
//};
