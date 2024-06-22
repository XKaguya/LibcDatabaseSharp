# LibcDatabaseSharp

LibcDatabaseSharp is a C# implementation of the libc-database, focusing on Ubuntu and Debian's libc files.

## Features:
- Search libc versions based on offsets. (Note: Due to stable release versions potentially corrupting the database, versions include a random number.)
  
![Search libc version](https://github.com/XKaguya/LibcDatabaseSharp/assets/96401952/d6cf64e5-4295-41b2-833d-3cc415705b1d)

## Details:
- Provides detailed information about libc versions.
  
![Details](https://github.com/XKaguya/LibcDatabaseSharp/assets/96401952/12384607-2a8b-4319-bfdb-bf552351864a)

## APIs:
- **GetOffsetByLibc**: Retrieves function offsets by libc name.
  
![GetOffsetByLibc](https://github.com/XKaguya/LibcDatabaseSharp/assets/96401952/3d790d49-aeea-496f-b85f-b45c68c4d730)

- **GetMatchingLibc**: Finds matching libc versions based on multiple arguments.
  
![GetMatchingLibc](https://github.com/XKaguya/LibcDatabaseSharp/assets/96401952/9cf05579-e624-4170-a624-3632a800d801)

Supports handling multiple arguments efficiently.

## APIs Usage
* GetMatchingLibc
```
Single Func Search
http://ip:port/api/Api/GetMatchingLibc?funcNames={funcName}&funcOffsets={funcOffset}

Multiple Func Search
http://ip:port/api/Api/GetMatchingLibc?funcNames={funcName1}&funcOffsets={funcOffset1}&funcNames={funcName2}&funcOffsets={funcOffset2}
```

* GetOffsetByLibc
```
http://ip:port/api/Api/GetOffsetByLibc?libcName={libcName}&funcNames={funcName}

libcName like: Ubuntu2.23-0ubuntu11.3
```

## Contribute

Contributions to LibcDatabaseSharp are welcome! Here's how you can contribute:

### Reporting Issues
If you encounter bugs or have suggestions for improvements, please [open an issue](https://github.com/XKaguya/LibcDatabaseSharp/issues) on GitHub. Include detailed information such as the steps to reproduce the issue and your environment setup.
