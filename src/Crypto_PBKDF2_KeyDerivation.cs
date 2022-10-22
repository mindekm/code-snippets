/* 
 * PBKDF2 requires that you select an internal hashing algorithm such as an HMAC or a variety of other hashing algorithms.
 * HMAC-SHA-256 is widely supported and is recommended by NIST.
 *     - https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html
 */
var algorithm = HashAlgorithmName.SHA256;

/*
 * The work factor for PBKDF2 is implemented through an iteration count, which should set differently based on the internal hashing algorithm used.
 *   PBKDF2-HMAC-SHA1:   720,000 iterations
 *   PBKDF2-HMAC-SHA256: 310,000 iterations
 *   PBKDF2-HMAC-SHA512: 120,000 iterations
 *     - https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html
 */
var iterations = 310_000;

/*
 * Valid AES key sizes: 128, 192, 256 bits.
 */
var keySizeBytes = 256 / 8;

var password = Encoding.UTF8.GetBytes("secret");
var salt = RandomNumberGenerator.GetBytes(32);

var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, algorithm, keySizeBytes);
