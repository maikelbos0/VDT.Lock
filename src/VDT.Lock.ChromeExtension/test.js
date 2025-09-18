import { dotnet } from './wasm/dotnet.js'

const { getAssemblyExports, getConfig } = await dotnet.create();
const config = getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);

document.getElementById('encryptionTest').addEventListener('click', async function () {
    const input = document.getElementById('encryptionInput').value;
    const result = await exports.VDT.Lock.TestClass.TestEncryption(input);

    document.getElementById("encryptionResult").innerText = result;
});

document.getElementById('storageTest').addEventListener('click', async function () {
    const input = document.getElementById('storageInput').value;
    const result = await exports.VDT.Lock.TestClass.TestChromeStorage(input);
    
    document.getElementById('storageResult').innerText = result;
});
