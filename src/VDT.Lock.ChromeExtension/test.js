import { dotnet } from './wasm/dotnet.js'

const { getAssemblyExports, getConfig } = await dotnet.create();
const config = getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);

document.getElementById('test').addEventListener('click', function () {
    const text = exports.VDT.Lock.TestClass.Test2();

    document.getElementById("result").innerText = text;
});
