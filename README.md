pvc-runtime-nodejs
==================

NodeJS runtime for PVC Build tasks and plugins

###What

Runtimes are just helpers to utilize other existing ecosystems from your code. This runtime provides access to NodeJS via the [Edge.js](https://github.com/tjanczuk/edge) library.

###How
You can pass in parameters and get results directly from JavaScript. In this example, we pass in a dynamic object that is available in the JavaScript as `pvc.data`.

All calls to runtimes are asynchronous so you must fire the callback function, `pvc.doneCallback` when the work is complete.

```
var browserifyBundle = PvcRuntimeNodeJs<PvcBrowserify>.Execute(
	new
	{
		entryPoint = Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileName(inputStream.StreamName))
	},
	@"
		var browserify = require('browserify');

		browserify(pvc.data.entryPoint)
			.bundle(function (err, output) {
				pvc.doneCallback(err, output);
			})
			.on('error', function (err) {
				console.error(err);
			});
	"
);
```

The output from browserify is passed back via `pvc.doneCallback` and is the result of the `Execute` method in C#. The return result is dynamic so you can return anything that can be serialized.

###NPM Packages
The base runtime packages can be used without any extra though, such as `fs`, `path` and `http`.

To give your plugin access to additional packages you have two methods.

1. Add a `package.json` file with a dependency block to your plugin project. Set it to be an Embedded Resource. In the constructor of your plugin, call `PvcRuntime<T>.InstallPackages()`.

	```
	{
	    "dependencies": {
	        "browserify":  "*"
	    }
	}
	```

	```
	PvcRuntimeNodeJs<PvcBrowserify>.InstallPackages();
	```
	
2. In the constructor of your plugin, call `PvcRuntime<T>.InstallPackages(packageName, ...)`

	```
	PvcRuntimeNodeJs<PvcBrowserify>.InstallPackages("browserify");
	```
