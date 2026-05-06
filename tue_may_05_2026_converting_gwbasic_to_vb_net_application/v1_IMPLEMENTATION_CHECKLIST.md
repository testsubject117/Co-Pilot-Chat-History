# Implementation Checklist (Itemized) — What Still Needs Wiring/Implementation

Date: 2026-05-04  
Scope priority: CHECKS menu first, then remaining Main Menu items.

Legend:
- Wiring: button/menu opens form or triggers flow
- Logic: underlying read/write rules, validation, file formats

---

## A) CHECKS Menu (FormLedgerMenu) — Missing Buttons

### A1) (1) Add Checks to ledger & cash receipts — NOT IMPLEMENTED
**DOS source:** LEDGER.BAS option (1)  
**VB.NET location:** AMiOffice/FormLedgerMenu.vb (button 1 currently NotYet)

**What must be built**
1) New form (DOS-style) to collect:
   - Company/customer code (3–8 chars, uppercase; DOS validated via PRC\\<CODE>.PRC existence)
   - Date (MM-DD-YYYY; DOS validated)
   - Check number
   - Check amount
   - Reference (routing/bank reference)
   - Salesman code (present in CHECK.INV header in VB reader; DOS uses EMPNAME.DAT for sales features)
   - Invoice list (manual entry) and/or auto-scan by invoice range
2) Writers:
   - LEDGER.CUR append (6 fields; field 4 blank; field 6 may include discount/debit suffix)
   - CHECK.INV append (header + CT invoice lines)
   - INVOICE.CHK updates: for each invoice, verify FLAG allows payment, then set FLAG="P"
3) Business rules to match:
   - Balance tolerance: ABS(sum(invoice amounts)-check amount) < 1
   - If out of balance: offer discount/debit marker appended to RF$ (field 6) like “(X.XX% Discount)” or “(Debit)”
   - Reject invoices with FLAG P/C/V; reject duplicates within same check
   - Auto-scan mode: scan INVOICE.CHK in invoice number range, include FLAG="J" and company matches, enforce DOS-ish minimum invoice number (DOS newer: >=150000)
4) Concurrency/locking:
   - DOS uses record LOCK/UNLOCK around INVOICE.CHK updates; VB.NET should serialize writes to avoid corruption on network share.

**Acceptance test**
- Add check creates visible row in View Checks.
- Selecting that row shows invoice details.
- Invoice flags in INVOICE.CHK reflect Paid.

---

### A2) (2) Delete Checks — NOT IMPLEMENTED
**DOS source:** LEDGER.BAS option (2)  
**VB.NET location:** AMiOffice/FormLedgerMenu.vb (button 2 currently NotYet)

**What must be built**
1) UI: select which check to delete (by customer+check#, and possibly date).
2) File operations:
   - Rebuild LEDGER.CUR excluding the check record(s)
   - Rebuild CHECK.INV excluding that check’s block
   - For invoices listed under that CHECK.INV block: set INVOICE.CHK flags back to "J"
3) Safety/atomicity:
   - DOS used temp files + size guards (>1000 bytes) then swap KILL/NAME.
   - VB.NET should do “write temp -> fsync -> replace” semantics; keep backups if possible.

**Acceptance test**
- Deleted check disappears from View Checks.
- Its CHECK.INV mapping is gone.
- Invoice flags revert to Unpaid ("J").

---

### A3) (6) Add OTHER Checks — NOT IMPLEMENTED
**DOS source:** LEDGER.BAS option (6)  
**VB.NET target:** new form + OtherChkWriter

**Rules**
- OTHER.CHK is 6-field sequential format.
- DOS validates date strictly (MM/DD ranges; year 1988–2099).
- Requires “Reason Why” field (required).

---

### A4) (7) Delete OTHER Checks — NOT IMPLEMENTED
**DOS source:** LEDGER.BAS option (7)  
**VB.NET target:** UI selection + rewrite OTHER.CHK excluding record(s)

Note: DOS used SHELL copy other.tmp other.chk (behavioral detail); VB can replace safely.

---

### A5) (A) Find Checks that don’t balance — NOT IMPLEMENTED (and DOS also doesn’t dispatch it)
**DOS note:** shown in menu but ignored (no handler)
**Decision needed:** implement a useful reconciliation report in VB.NET

**Suggested VB.NET behavior**
- For each ledger entry that has CHECK.INV mapping:
  - Compute sum(invoice amounts from INVOICE.CHK)
  - Compare to ledger amount with tolerance < 1
  - Show list of mismatches with delta and percent
- For ledger entries without mapping: label “No invoice mapping”

---

### A6) (S) Sales Employee’s checks — NOT IMPLEMENTED
**DOS source:** LEDGER.BAS option (S), uses EMPNAME.DAT + INVOICE.CHK
**What must be built**
- Reader for EMPNAME.DAT format (employee names + indented customers)
- Commission computation:
  - default 15%
  - employee name starts with "STEV" => 10%
- Output:
  - On-screen report first (grid)
  - optional export (txt/csv)
  - DOS appended to Word docs + print; in VB.NET we can add “Export” and “Print” later

---

## B) CHECKS Menu (Already Wired) — Behavior alignment items

### B1) View Checks prompt semantics (customer/check/year/month)
**DOS:** 4 prompts; month only asked if year provided  
**VB.NET current:** 3 prompts; month is on-screen filter combo

**Action**
- Decide if we want DOS-exact prompting (add Month prompt when Year entered) or keep current UI (month dropdown).
- Ensure typed prompt values are applied as initial filters when provided; blank continues to mean ALL.

---

## C) Main Menu — Remaining Wiring (stubs)
These are still NotYet on FormMainMenu (per your earlier scan):
A, B, D, E, F, G, H, J, K, L, M, N, O, P, Q, R, T, X, Y, 2, 3, 4, 6, 7

DOS mapping provides the authoritative targets (SHOPCARD, INVOICE_, SALES, LOGBOOK, PLIST, BOOT, BILL, ENTIRE, etc.).
Recommendation: implement in descending value order after CHECKS is complete:
1) B Invoice Generator
2) G Print/Void Invoices (BOOT)
3) P Entire Ledger Viewing (ENTIRE)
4) D/E Sales Journal / Logbook
5) K Cash Disbursements (BILL)
…then the rest.
