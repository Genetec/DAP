// Copyright 2026 Genetec Inc.
// Licensed under the Apache License, Version 2.0

namespace Genetec.Dap.CodeSamples;

using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

internal static class CredentialStore
{
    private const uint GenericCredential = 1;
    private const int NotFound = 1168;

    // Include the SDK port so distinct Directories on the same host do not share a secret.
    internal static string GetTarget(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory) ||
            !Uri.TryCreate("gsc://" + directory.Trim(), UriKind.Absolute, out var uri) ||
            string.IsNullOrEmpty(uri.Host) || !string.IsNullOrEmpty(uri.UserInfo) ||
            (uri.AbsolutePath != "" && uri.AbsolutePath != "/") ||
            !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
            throw new ArgumentException("Enter a Directory hostname or IP address, with an optional port.");

        var host = uri.IdnHost.ToLowerInvariant();
        if (uri.HostNameType == UriHostNameType.IPv6)
            host = "[" + host.Trim('[', ']') + "]";

        return "Genetec.DAP.CredentialLogon/" + host + ":" + (uri.Port < 0 ? 5500 : uri.Port);
    }

    internal static NetworkCredential Read(string directory)
    {
        if (!CredRead(GetTarget(directory), GenericCredential, 0, out var pointer))
        {
            var error = Marshal.GetLastWin32Error();
            if (error == NotFound)
                return null;
            throw new Win32Exception(error);
        }

        try
        {
            var credential = Marshal.PtrToStructure<NativeCredential>(pointer);
            if (credential.CredentialBlobSize > 2560 || credential.CredentialBlobSize % 2 != 0 ||
                (credential.CredentialBlobSize != 0 && credential.CredentialBlob == IntPtr.Zero))
                throw new InvalidOperationException("The saved credential has an invalid password format. Forget it and save a new credential.");

            var password = credential.CredentialBlobSize == 0 ? string.Empty :
                Marshal.PtrToStringUni(credential.CredentialBlob, (int)credential.CredentialBlobSize / 2);
            return new NetworkCredential(credential.UserName, password);
        }
        finally
        {
            CredFree(pointer);
        }
    }

    internal static void Write(string directory, string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Enter a username.");
        if (password == null || Encoding.Unicode.GetByteCount(password) > 2560)
            throw new ArgumentException("The password exceeds the Windows Credential Manager limit.");

        var target = GetTarget(directory);
        var pointer = Marshal.StringToCoTaskMemUni(password);
        try
        {
            var credential = new NativeCredential
            {
                Type = GenericCredential,
                TargetName = target,
                UserName = username,
                CredentialBlob = pointer,
                CredentialBlobSize = (uint)Encoding.Unicode.GetByteCount(password),
                Persist = 2 // Persist on this computer for the current Windows user.
            };
            if (!CredWrite(ref credential, 0))
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }
        finally
        {
            Marshal.ZeroFreeCoTaskMemUnicode(pointer);
        }
    }

    internal static bool Delete(string directory)
    {
        if (CredDelete(GetTarget(directory), GenericCredential, 0))
            return true;
        var error = Marshal.GetLastWin32Error();
        if (error == NotFound)
            return false;
        throw new Win32Exception(error);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NativeCredential
    {
        public uint Flags, Type;
        public string TargetName, Comment;
        public long LastWritten;
        public uint CredentialBlobSize;
        public IntPtr CredentialBlob;
        public uint Persist, AttributeCount;
        public IntPtr Attributes;
        public string TargetAlias, UserName;
    }

    [DllImport("advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CredRead(string target, uint type, uint flags, out IntPtr credential);

    [DllImport("advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CredWrite(ref NativeCredential credential, uint flags);

    [DllImport("advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CredDelete(string target, uint type, uint flags);

    [DllImport("advapi32.dll")]
    private static extern void CredFree(IntPtr credential);
}
