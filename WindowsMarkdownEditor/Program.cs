﻿using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using WindowsMarkdownEditor;

/// <summary>
/// Our own magical starting point to be called before ordinary App.xml created starting point.
/// This loads embeded resources and hands over to App.Main().
/// 
/// For details on this:
/// http://www.digitallycreated.net/Blog/61/combining-multiple-assemblies-into-a-single-exe-for-a-wpf-application
/// </summary>
public class Program
{
    [STAThreadAttribute]
    public static void Main()
    {
        AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
        App.Main();
    }

    private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
    {
        Assembly executingAssembly = Assembly.GetExecutingAssembly();
        AssemblyName assemblyName = new AssemblyName(args.Name);

        string path = assemblyName.Name + ".dll";
        if (assemblyName.CultureInfo.Equals(CultureInfo.InvariantCulture) == false)
        {
            path = String.Format(@"{0}\{1}", assemblyName.CultureInfo, path);
        }

        using (Stream stream = executingAssembly.GetManifestResourceStream(path))
        {
            if (stream == null)
                return null;

            byte[] assemblyRawBytes = new byte[stream.Length];
            stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
            return Assembly.Load(assemblyRawBytes);
        }
    }
}