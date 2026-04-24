# 📦 Inventory Management Web Application

> Course project for C# / ASP.NET (.NET 8.0)

---

## 🚀 Overview

A web application for managing inventories (office equipment, books, HR documents, etc.).

Users can create custom inventory templates with flexible fields and manage items within them.

---

## 🧩 Tech Stack

- **Backend:** C# / ASP.NET (.NET 8.0)
- **Database:** SQL Server / MySQL / PostgreSQL
- **ORM:** Entity Framework
- **Frontend:** Bootstrap (or any CSS framework)
- **Other:** Third-party libraries allowed

---

## ✨ Features

### 📋 Inventories
- Create inventories with:
  - Title
  - Description (Markdown supported)
  - Category
  - Tags (with autocomplete)
  - Image (cloud upload)

- Access control:
  - Public (all authenticated users can edit)
  - Private (specific users only)

---

### 🧾 Items
- Created based on inventory structure
- Permissions:
  - Everyone can view
  - Only authorized users can edit

---

### 🆔 Custom IDs (Killer Feature #1)

Each inventory defines its own ID format.

Supported elements:
- Fixed text
- Random numbers (20-bit, 32-bit, 6-digit, 9-digit)
- GUID
- Date/time
- Sequence number

Features:
- Drag & drop configuration
- Real-time preview
- Unique within inventory (DB constraint)
- Editable with validation

---

### 🧱 Custom Fields (Killer Feature #2)

Each inventory can define custom fields:

| Type              | Limit |
|------------------|------|
| Single-line text | 3    |
| Multi-line text  | 3    |
| Numeric          | 3    |
| Image/Doc (URL)  | 3    |
| Boolean          | 3    |

Each field includes:
- Name
- Description (tooltip)
- Visibility in table

---

### 📊 Table-based UI (Required)

- All data displayed in tables
- No buttons inside rows ❌
- Use toolbar / contextual actions ✔

---

### 🔍 Search

- Full-text search
- доступен на каждой странице

---

### 👤 Authentication

- OAuth login:
  - Google
  - Facebook

---

### 🧑‍💼 Admin Panel

Admins can:
- Block / unblock users
- Delete users
- Grant/remove admin role
- Manage all inventories

⚠️ Admin can remove their own admin role

---

### 👤 User Profile

Two tables:
- Owned inventories
- Accessible inventories

Supports sorting & filtering.

---

### 📄 Inventory Page

Tabs:
1. Items
2. Discussion
3. Settings
4. Custom ID
5. Access
6. Fields
7. Statistics

---

### 💬 Discussion

- Real-time updates (2–5 sec)
- Markdown support
- User profiles linked

---

### 👍 Likes

- One like per user per item

---

### ⚙️ Auto-save

- Every 7–10 seconds
- Optimistic locking

---

### 📈 Statistics

- Item count
- Averages & ranges
- Most frequent values

---

### 🌍 Localization

- English + one additional language

---

### 🎨 Themes

- Light / Dark mode
- Persisted user preference

---

## 🏠 Main Page

- Latest inventories
- Top 5 popular inventories
- Tag cloud

---

## 🧠 Architecture Notes

✔ Use ORM  
✔ Use full-text search  

❌ Avoid:
- `SELECT *`
- Queries inside loops
- Storing images locally
- JSON storage for items
- Dynamic DB tables
- Buttons inside table rows

---

## ⭐ Optional Features

- Document previews (PDF/JPG)
- Email authentication
- Field validation (regex, ranges)
- Dropdown fields
- Unlimited fields
- Export to CSV/Excel

---

## ⚠️ Important

- Do NOT copy code
- Understand everything you write
- Use libraries instead of reinventing

---

## 🟢 Development Strategy

Start with:

```bash
Hello World → Deploy → Incremental development
