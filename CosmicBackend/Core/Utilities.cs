using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Runtime.CompilerServices;

namespace CosmicBackend.Core
{
    internal static class Utilities
    {
        internal static string HashPassword(string input)
        {
            using SHA256 hashGen = SHA256.Create();
            return ByteArrayToHexadecimalString(hashGen.ComputeHash(Encoding.UTF8.GetBytes(input)));
        }

        internal static string GetPathPart(string path) => path.Substring(0, path.LastIndexOf("/") + 1);

        internal static JsonResult SetError(string error, string errorCode, int statusCode)
        {
            var errorMessage = new
                                   {
                                       error,
                                       errorCode,
                                       statusCode
                                   };

            return new JsonResult(errorMessage);
        }

        internal static (string, ulong) DissectExchange(string exchange)
        {
            if (string.IsNullOrEmpty(exchange) || exchange.Length < 26)
            {
                return (null, 0);
            }

            return (exchange.Substring(0, 24),
             ByteArrayToULong(HexadecimalStringToByteArray(exchange.Substring(24)), 0) ^ ulong.MaxValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe TOutput ReinterpretCast<TValue, TOutput>(TValue value)
            where TValue : unmanaged where TOutput : unmanaged =>
            ReinterpretCast<TOutput>((nint)(&value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe TOutput ReinterpretCast<TOutput>(nint ptr)
            where TOutput : unmanaged
        {
            if (ptr == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(ptr));
            }

            return *(TOutput*)ptr;
        }

        internal static unsafe byte[] ToByteArray<T>(T value)
            where T : unmanaged
        {
            switch (value)
            {
                case byte byteValue:
                    return new byte[1] { byteValue };
                case bool boolValue:
                    return new byte[1] { boolValue ? 1 : 0 };
            }

            int length = sizeof(T);
            byte[] buffer = new byte[length];
            byte*
                pValue = (byte*)&value; // if we don't cast to (byte*), pValue++ will increment the memory offset by sizeof(T)
            fixed (byte* pBuffer = buffer)
            {
                byte* bBuffer = pBuffer;
                for (int i = 0; i < length; i++)
                {
                    *bBuffer++ = *pValue++;
                }
            }

            return buffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe string ByteArrayToHexadecimalString(ReadOnlySpan<byte> arr)
        {
            if (arr == null)
            {
                throw new ArgumentNullException(nameof(arr));
            }

            int length = arr.Length * 2;
            switch (length)
            {
                case 0:
                    return string.Empty;
                case 1:
                    return ByteToHexChar(arr[0]).ToString();
            }

            char* cBuffer = stackalloc char[length * sizeof(char)];
            fixed (byte* pArr = arr)
            {
                ByteArrayToHexadecimalStringInternal(arr.Length, cBuffer, pArr);
            }

            return new(cBuffer);
        }

        internal static byte[] HexadecimalStringToByteArray(string hexString)
        {
            _ = hexString ?? throw new ArgumentNullException(nameof(hexString));
            int length = hexString.Length;
            bool isEven = length % 2 == 0;
            switch (length)
            {
                case 0:
                    return Array.Empty<byte>();
                case 1:
                    return new byte[1] { HexCharToByte(hexString[0]) };
                case 2:
                    return new byte[1] { GetByteFromHexadecimalChars(hexString) };
            }

            byte[] buffer;
            int boundary;
            if (isEven)
            {
                buffer = new byte[length / 2];
                boundary = 2;
            }
            else
            {
                buffer = new byte[length / 2 + 1];
                boundary = 1;
            }

            for (int i = 0; i < length / 2 - boundary; i++)
            {
                buffer[i] = GetByteFromHexadecimalChars(hexString.Substring(i * 2, 2));
            }

            if (isEven)
            {
                buffer[^2] = GetByteFromHexadecimalChars(hexString.Substring(length - 4, 2));
                buffer[^1] = GetByteFromHexadecimalChars(hexString.Substring(length - 2, 2));
            }
            else
            {
                buffer[^2] = GetByteFromHexadecimalChars(hexString.Substring(length - 3, 2));
                buffer[^1] = HexCharToByte(hexString[^1]);
            }

            return buffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static char ByteToHexChar(byte rawByte)
        {
            if (rawByte > 15)
            {
                throw new ArgumentException($"Byte {rawByte} is not a valid hex byte [0-15].", nameof(rawByte));
            }

            return (char)(rawByte > 9 ? rawByte - 10 + 'a' : rawByte + '0');
        }

        private static unsafe void ByteArrayToHexadecimalStringInternal(int length, char* cBuffer, byte* bBuffer)
        {
            byte rawByte;
            for (int i = 0; i < length; i++)
            {
                rawByte = (byte)(*bBuffer >> 0x04);
                *cBuffer++ = ByteToHexChar(rawByte);
                rawByte = (byte)(*bBuffer++ & 0x0F);
                *cBuffer++ = ByteToHexChar(rawByte);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte HexCharToByte(char rawChar)
        {
            if (!IsValidHexChar(rawChar))
            {
                throw new ArgumentException($"Char {rawChar} is not a valid hex char [0-f].", nameof(rawChar));
            }

            return (byte)(rawChar > '9' ? rawChar - 'a' + 10 : rawChar - '0');
        }

        private static unsafe byte GetByteFromHexadecimalChars(string hexChars)
        {
            _ = hexChars ?? throw new ArgumentNullException(nameof(hexChars));

            if (hexChars.Length != 2)
            {
                throw new ArgumentException(
                    "Invalid string input length.\n" + "Expected: 2" + $"Got: {hexChars.Length}",
                    nameof(hexChars));
            }

            byte* bBuffer = stackalloc byte[2];
            fixed (char* pHexChars = hexChars)
            {
                char* cHexChars = pHexChars;
                for (int i = 0; i < 2; i++)
                {
                    bBuffer[i] = HexCharToByte(*cHexChars++);
                }
            }

            return (byte)((*bBuffer * 16) + *(bBuffer + 1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsValidHexChar(char c) =>
            c switch
                {
                    >= '0' and <= '9' => true, //[0-9]
                    >= 'A' and <= 'F' => true, //[A-F]
                    >= 'a' and <= 'f' => true, //[a-f]
                    _ => false
                };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong ByteArrayToULong(ReadOnlySpan<byte> arr, byte index) => ByteArrayTo<ulong>(arr, index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe T ByteArrayTo<T>(ReadOnlySpan<byte> arr, byte index)
            where T : unmanaged
        {
            if (arr == null)
            {
                throw new ArgumentNullException(nameof(arr));
            }

            int length = arr.Length - index;
            if (length < sizeof(T))
            {
                throw new ArgumentException(
                    "Input array is with specified offset is too small.\n" + $"Expected: {sizeof(T)}\n"
                                                                           + $"Got: {length}",
                    nameof(length));
            }

            fixed (byte* pArr = arr)
            {
                return ReinterpretCast<T>((nint)pArr + index);
            }
        }
    }
}