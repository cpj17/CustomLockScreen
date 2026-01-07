# CustomLockScreen
Custom lock screen app made by CPJ. Using Winforms in KioskApp method

Below is a clean, professional **`README.md`** you can directly place in your GitHub repository.
It explains what the project does, features, setup, configuration, and security notes based on your code.

---

````md
# 🔒 Custom Lock Screen (Windows Forms)

A full-screen **custom Windows lock screen** built using **C# WinForms**.  
This application prevents user interaction until the correct password is entered, featuring smooth animations, attempt limits, and a secure hashed password system.

---

## ✨ Features

- 🖥️ Full-screen, borderless, always-on-top lock screen
- ⏰ Live system clock (12-hour format with AM/PM)
- 🔐 Password-protected unlock (MD5 hash-based)
- 🔑 Hidden admin unlock shortcut (**Ctrl + Shift + U**)
- 🎞️ Smooth fade-out unlock animation
- ❌ Shake animation on incorrect password
- 🚫 Lockout after 5 invalid attempts
- ⌨️ Keyboard hook to block system shortcuts
- ⚙️ Password stored securely in `App.config`

---

## 🛠️ Technologies Used

- **C#**
- **.NET Framework / WinForms**
- **MD5 Hashing**
- **Windows API (Keyboard Hook)**

---

## 🚀 Getting Started

### 1️⃣ Clone the Repository

```bash
git clone https://github.com/your-username/CustomLockScreen.git
````

### 2️⃣ Open in Visual Studio

* Open the `.sln` file
* Ensure the target framework matches your installed .NET version

### 3️⃣ Configure Password

In `App.config`, set the hashed password:

```xml
<appSettings>
  <add key="hashedUnlockPassword" value="YOUR_MD5_HASH_HERE" />
</appSettings>
```

To generate an MD5 hash:

```csharp
Md5Helper.ComputeMd5("your-password");
```

---

## 🔐 Default Credentials

| Type         | Password                             |
| ------------ | ------------------------------------ |
| User Unlock  | Defined in `App.config` (MD5 hashed) |
| Admin Unlock | `1234`                               |

⚠️ **Change the admin password before production use.**

---

## 🎯 How It Works

1. App launches in **full-screen lock mode**
2. Keyboard shortcuts and closing are blocked
3. User enters password
4. If correct → fade-out animation → exit
5. If incorrect → shake animation
6. After 5 failed attempts → permanent lock screen

---

## ⌨️ Admin Unlock Shortcut

Press:

```
CTRL + SHIFT + U
```

Enter the **admin password** to unlock immediately.

---

## ⚠️ Security Notes

* MD5 is used for simplicity but **not recommended** for high-security environments
* For production, consider:

  * `SHA-256` or `PBKDF2`
  * Windows Credential Manager
  * Encrypted config sections

---

## 📸 UI Preview

* Centered username
* Large digital clock
* Password input with unlock button

---

## 📄 License

This project is licensed under the **MIT License**.
You are free to modify and distribute it.

---

## 👤 Author

**CPJ**

---
