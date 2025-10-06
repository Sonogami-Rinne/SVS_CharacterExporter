The process.py is a Python script.

It process bundles to make accessory meshes readable.

Currently, this script only process bundles that original game have.

It do NOT process bundles that added by mod.

You need to use pyinstaller to turn this py file into exe.

For example: 
```bash
pyinstaller -F process.py --add-data "C:\Users\Administrator\AppData\Local\Packages\PythonSoftwareFoundation.Python.3.9_qbz5n2kfra8p0\LocalCache\local-packages\Python39\site-packages\UnityPy\resources;UnityPy\resources"
```

Adjust the parameters based on your own situation.

Or you can use python.exe to execute the script directly.

---

Process.py 是一个Python脚本。

它处理特定的unity3d包，让部分饰品的网格对象可读。

目前，这个脚本只处理游戏自带的unity3d包

它不处理mod加的unity3d包

你需要使用pyinstaller来把该脚本编译为exe文件

例如: 
```bash
pyinstaller -F process.py --add-data "C:\Users\Administrator\AppData\Local\Packages\PythonSoftwareFoundation.Python.3.9_qbz5n2kfra8p0\LocalCache\local-packages\Python39\site-packages\UnityPy\resources;UnityPy\resources"
```
根据你自己的情况来调整参数

或者直接使用python.exe来执行那个脚本
