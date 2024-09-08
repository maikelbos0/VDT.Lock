# VDT.Lock

After all... Why not? Why shouldn't I try to create a password manager?

## Password managers

Even though I understand the importance of using password managers so I can use a unique, strong password for every place I create an account, I have a hard
time trusting existing password managers for two reasons:

- I don't properly understand how they work
- I don't trust programmers because I know how easy it is to mess a small detail up

So, in an attempt to understand them better, instead of reviewing the code of an existing password manager, I've decided to build my own.

## Ideas and concepts

- The main library will be written in C# and compiled to IL for .NET MAUI and to WASM for browser extensions
- While storage sites should be able to verify the identity of the data store, it should not have access to either the master password or the data contents
- Sensitive fields should be encrypted in memory and only be accessible for reading via streams to prevent the strings being accessible in memory
- Personal data stores are stores that have a master password and belong to a user
- Shared data stores are stores that can be shared between many users while maintaining the integrity of the individual user identities for verification
