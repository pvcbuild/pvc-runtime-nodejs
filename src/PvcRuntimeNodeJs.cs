using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PvcRuntime.NodeJs
{
    public class PvcRuntimeNodeJs<TPlugin>
    {
        public PvcRuntimeNodeJs()
        {
        }

        public static dynamic Execute(dynamic data, string script)
        {
            return Execute<dynamic>(data, script);
        }

        public static TResult Execute<TResult>(dynamic data, string script)
        {
            var edgeFunc = EdgeJs.Edge.Func(@"
                var fixture = require('fixture-stdout');
                var path = require('path');

                return function() {
                    pvc = arguments[0];
                    pvc.lib = require('pvc-nodelib');
                    pvc.doneCallback = arguments[arguments.length - 1];

//                    new fixture().capture(function (str) {
//                        pvc.onStreamOutput(str);
//                        return false;
//                    });
//
//                    new fixture({ stream: process.stderr }).capture(function (str) {
//                        pvc.onStreamOutput(str);
//                        return false;
//                    });

                    var scriptDirectory = process.cwd();
                    (function (require) {
                    " + script + @"
                    })(function (moduleName) {
                        var resultModule;

                        try {
                            resultModule = require(path.join(scriptDirectory, 'pvc-packages', 'npm', pvc.scopeName, 'node_modules', moduleName));
                        }
                        catch(e) {
                            resultModule = require(moduleName);
                        }

                        return resultModule;
                    });
                }
            ");

            var task = edgeFunc(new NodeJsInputData(typeof(TPlugin).Assembly.GetName().Name, data));
            task.Wait();

            return (TResult)task.Result;
        }

        public static void InstallPackages()
        {
            var packageJson = LoadPackageJsonFromAssembly();

            PvcRuntimeNodeJs<TPlugin>.Execute(new
            {
                packageJson = packageJson
            },
            @"
                pvc.lib.npm.install(JSON.parse(pvc.data.packageJson), function (err, data) {
                    pvc.doneCallback(err, data);
                });
            ");
        }

        public static void InstallPackages(params string[] packageNames)
        {
            PvcRuntimeNodeJs<TPlugin>.Execute(new
            {
                packageNames = packageNames
            },
            @"
                pvc.lib.npm.installPackages(pvc.data.packageNames, function (err, data) {
                    pvc.doneCallback(err, data);
                });
            ");
        }

        private static string LoadPackageJsonFromAssembly()
        {
            var assembly = typeof(TPlugin).Assembly;
            var resourceName = "package.json";
            resourceName = assembly.GetManifestResourceNames().FirstOrDefault(x => x.EndsWith(resourceName));

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
