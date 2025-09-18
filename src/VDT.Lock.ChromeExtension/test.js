import { dotnet } from './wasm/dotnet.js'

const { getAssemblyExports, getConfig } = await dotnet.create();
const config = getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);

document.getElementById('encryptionTest').addEventListener('click', async function () {
    const text = await exports.VDT.Lock.TestClass.StorageTest();

    document.getElementById("encryptionResult").innerText = text;
});

document.getElementById('storageTest').addEventListener('click', async function () {
    const value = "Store me";

    await chrome.storage.local.set({ value: value });

    var storedValue = await chrome.storage.local.get('value');
    
    document.getElementById('storageResult').innerText = storedValue.value;
});
