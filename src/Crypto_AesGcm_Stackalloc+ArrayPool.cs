/*
* Data structure
* 
*  -----------------------------------------------------------------------------------------------------------
* | Nonce section                              | Tag section                             | Ciphertext section |
*  -----------------------------------------------------------------------------------------------------------
* | Nonce size | Nonce                         | Tag size  | Tag                         | Ciphertext         |
*  -----------------------------------------------------------------------------------------------------------
* | Int32Size  | AesGcm.NonceByteSizes.MaxSize | Int32Size | AesGcm.TagByteSizes.MaxSize | data.Length        |
*  -----------------------------------------------------------------------------------------------------------
* | 4 bytes    | X (12) bytes                  | 4 bytes   | Y (16) bytes                | Z bytes            |
*  -----------------------------------------------------------------------------------------------------------
*  
*/

private const int Int32Size = sizeof(int);

// "Because the amount of memory available on the stack depends on the environment in which the code is executed,
// be conservative when you define the actual limit value."
//    - https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/stackalloc
private const int MaxStackAllocation = 1024;

public static string EncryptToBase64(string data, ReadOnlySpan<byte> key)
{
    return EncryptToBase64(Encoding.UTF8.GetBytes(data), key);
}

public static string DecryptFromBase64(string encryptedData, ReadOnlySpan<byte> key)
{
    return Decrypt(Convert.FromBase64String(encryptedData), key);
}

[SkipLocalsInit]
public static string EncryptToBase64(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key)
{
    var nonceSize = AesGcm.NonceByteSizes.MaxSize;
    var tagSize = AesGcm.TagByteSizes.MaxSize;

    var nonceSectionSize = nonceSize + Int32Size;
    var tagSectionSize = tagSize + Int32Size;
    var ciphertextSectionSize = data.Length;

    var finalSize = nonceSectionSize + tagSectionSize + ciphertextSectionSize;
    byte[] rentedArray = default;
    var buffer = finalSize < MaxStackAllocation
        ? stackalloc byte[finalSize]
        : rentedArray = ArrayPool<byte>.Shared.Rent(finalSize);

    try
    {
        var encryptedData = buffer[..finalSize];
        encryptedData.Clear();

        // Prepare nonce section
        BinaryPrimitives.WriteInt32LittleEndian(encryptedData[..Int32Size], nonceSize);
        var nonce = encryptedData.Slice(Int32Size, nonceSize);
        RandomNumberGenerator.Fill(nonce);

        // Prepare tag section
        BinaryPrimitives.WriteInt32LittleEndian(encryptedData.Slice(nonceSectionSize, Int32Size), tagSize);
        var tag = encryptedData.Slice(nonceSectionSize + Int32Size, tagSize);

        // Prepare ciphertext section
        var ciphertext = encryptedData.Slice(nonceSectionSize + tagSectionSize, ciphertextSectionSize);

        // AesGcm class is ***NOT*** thread-safe.
        using var aes = new AesGcm(key);
        aes.Encrypt(nonce, data, ciphertext, tag);

        return Convert.ToBase64String(encryptedData);
    }
    finally
    {
        if (rentedArray is not null)
        {
            ArrayPool<byte>.Shared.Return(rentedArray);
        }
    }
}

[SkipLocalsInit]
public static string Decrypt(ReadOnlySpan<byte> encryptedData, ReadOnlySpan<byte> key)
{
    // Retrieve nonce section
    var nonceSectionStart = 0;
    var nonceValueStart = nonceSectionStart + Int32Size;
    var nonceSize = BinaryPrimitives.ReadInt32LittleEndian(encryptedData.Slice(nonceSectionStart, Int32Size));
    var nonceSectionSize = Int32Size + nonceSize;
    var nonce = encryptedData.Slice(nonceValueStart, nonceSize);

    // Retrieve tag section
    var tagSectionStart = nonceSectionSize;
    var tagValueStart = tagSectionStart + Int32Size;
    var tagSize = BinaryPrimitives.ReadInt32LittleEndian(encryptedData.Slice(tagSectionStart, Int32Size));
    var tagSectionSize = Int32Size + tagSize;
    var tag = encryptedData.Slice(tagValueStart, tagSize);

    // Retrieve ciphertext section
    var ciphertextSectionStart = nonceSectionSize + tagSectionSize;
    var ciphertextSize = encryptedData.Length - ciphertextSectionStart;
    var ciphertext = encryptedData.Slice(ciphertextSectionStart, ciphertextSize);

    byte[] rentedArray = default;
    var buffer = ciphertextSize < MaxStackAllocation
        ? stackalloc byte[ciphertextSize]
        : rentedArray = ArrayPool<byte>.Shared.Rent(ciphertextSize);

    try
    {
        var plaintext = buffer[..ciphertextSize];
        plaintext.Clear();

        // AesGcm class is ***NOT*** thread-safe.
        using var aes = new AesGcm(key);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }
    finally
    {
        if (rentedArray is not null)
        {
            ArrayPool<byte>.Shared.Return(rentedArray);
        }
    }
}
