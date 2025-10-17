import { dotnet } from './wasm/dotnet.js'

const { getAssemblyExports, getConfig } = await dotnet.create();
const config = getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);

document.getElementById('encryptionTest').addEventListener('click', async function () {
    const input = document.getElementById('encryptionInput').value;
    const result = await exports.VDT.Lock.TestClass.TestEncryption(input);

    document.getElementById("encryptionResult").innerText = result;
});

document.getElementById('chromeStorageTest').addEventListener('click', async function () {
    const input = document.getElementById('chromeStorageInput').value;
    const result = await exports.VDT.Lock.TestClass.TestChromeStorage(input);
    
    document.getElementById('chromeStorageResult').innerText = result.substring(0, 25) + "...";
});

document.getElementById('apiStorageTest').addEventListener('click', async function () {
    console.log(exports);
    const input = document.getElementById('apiStorageInput').value;
    const result = await exports.VDT.Lock.TestClass.TestApiStorage(input);

    document.getElementById('apiStorageResult').innerText = result.substring(0, 25) + "...";
});
