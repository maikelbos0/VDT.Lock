import { dotnet } from './wasm/dotnet.js'

const { getAssemblyExports, getConfig } = await dotnet.create();
const config = getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);

document.getElementById('test').addEventListener('click', async function () {
    const text = await exports.VDT.Lock.TestClass.Test();

    document.getElementById("result").innerText = text;
});
