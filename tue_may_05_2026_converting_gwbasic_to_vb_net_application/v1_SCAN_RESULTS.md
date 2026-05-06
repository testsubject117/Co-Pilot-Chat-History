# Repo Deep-Scan Results (AmiWinForms + AMiDataStore)

Date: 2026-05-04  
Repos:
- testsubject117/AmiWinForms (VB.NET WinForms)
- testsubject117/AMiDataStore (DOS GW-BASIC)

---

## 1) AMiDataStore (DOS) — Main Menu Dispatch
Entry: MAINMENU.BAS / MAINM.BAS (MAINM.ASC)

Key -> Action -> Target
- A  Shop Card Generator -> CHAIN "SHOPCARD" -> SHOPCARD.BAS
- B  Invoice Generator -> SHELL "QB /RUN INVOICE /MBF" -> INVOICE_.BAS
- C  Add Checks to Cash Receipts -> CHAIN "LEDGER" -> LEDGER.BAS
- D  View Sales Journal -> CHAIN "SALES" -> SALES.BAS
- E  View Log Book -> CHAIN "LOGBOOK" -> LOGBOOK.BAS
- F  Price List Program -> CHAIN "plist" -> PLIST.BAS
- G  Print Records, Void Invoices -> CHAIN "BOOT" -> BOOT.BAS
- H  Quick Message Flashing -> internal (password “dean”)
- I  Backup Price List & Rolodex -> internal ZIP backup routine
- J  Print Customers' Actual Names -> internal (REALNAME.DAT)
- K  Cash Disbursements -> BILL.BAS (compiled EXE)
- L  Business Expenses Account -> internal password EDLAR -> PERSONAL.BAS
- M  Quotation Form Generator -> QUOTE.BAS
- N  Rolodex -> PHONE.BAS (+ shell/chdir)
- O  Copy Spec Index -> internal copy DOC files
- P  Entire Ledger Viewing -> ENTIRE.BAS
- Q  Word Processor -> external WORD.EXE
- R  Find Word Processor text -> external TS.EXE
- S  Clean Keyboard -> internal
- T  Change Date or Time -> internal
- U  Phone Line for Dean -> SYSTEM exit
- V  Change Main Menu Message -> writes NOTES-4.ED
- W  Leave a Phone Message -> PHMESAGE.BAS
- X  Typewriter Mode -> internal
- Y  Ed Dean's Personal Backup -> EDBACKUP.BAS
- Z  Personal Calendar -> external PMS.EXE
- 1  Mileage Tracking -> MILEAGE.BAS
- 2  Product Purchasing -> PURCHASE.BAS
- 3  Miscellaneous Menu -> external MENU.EXE
- 4  Add Entries to Log Book -> LOGENTER.BAS
- 5  VGA Color Fonts -> VGAFONTS.BAS

---

## 2) AMiDataStore — LEDGER.BAS (“Checks and Cash Receipts”) Sub-Menu
Menu:
(1) Add Checks to ledger & cash receipts  -> line ~700
(2) Delete Checks                          -> line ~1620
(3) View Checks                            -> line ~460
(5) View Companys Totals                   -> line ~1890
(6) Add OTHER Checks                       -> line ~2100
(7) Delete OTHER Checks                    -> line ~3250
(8) View OTHER Checks                      -> line ~2410
(9) Find a Check #                         -> line ~2870
(0) Find an Invoice #                      -> line ~3060
(A) Find Checks that don't balance         -> DISPLAYED but NOT DISPATCHED in DOS (ignored)
(S) Sales Employee's checks                -> line ~7600
(Q) Quit to Main Menu                      -> CHAIN MAINMENU

Important: DOS bug/feature — option (A) is printed but not implemented.

---

## 3) AMiDataStore — “View Checks” Prompt Semantics (Option 3)
Prompts:
1) Customer: Enter = all customers -> DOS uses N$="A" for ALL, else truncates to 8 chars
2) Check#:   Enter = all checks    -> DOS uses CKN$="A" for ALL
3) Year:     Enter = all years     -> if blank, month prompt is skipped
4) Month:    only asked if year provided; Enter=all months; pads to 2 digits

Filters:
- Year compare uses last 2 digits: RIGHT$(YEAR$,2) vs RIGHT$(DT$,2)
- Month compare uses LEFT$(DT$,2) vs MTH$ when provided
Date string is "MM-DD-YYYY" in DOS.

---

## 4) AMiDataStore — Data File Formats / Business Rules (Checks Domain)
LEDGER.CUR (sequential text; 6 fields/record, each its own line in current VB reader)
- 1: Company code (max 8 chars, uppercase)
- 2: Date "MM-DD-YYYY"
- 3: Check number (string)
- 4: always "" in current DOS writes
- 5: amount numeric
- 6: reference text; may include “(X.XX% Discount)” or “(Debit)” suffix from balancing process

CHECK.INV (sequential text)
- Header line: N2$, CNUM$, RF$, DT$, CHKAMT, CT
- Next CT lines: invoice numbers

INVOICE.CHK (random access binary, 26 bytes/record)
- Invoice record index and amount stored in MBF single in first 4 bytes of two fields (VB.NET already has MBF decoder)
- CO$ (8 bytes), FLAG (1 byte)
- Flags: J unpaid, P paid, C corrected cert, V void, E erased

Other key DOS rules:
- Add Check: can auto-scan unpaid invoices (FLAG="J") within range; newer min invoice number 150000.
- Add Check: balancing tolerance ABS(TOT-CHKAMT)<1; out-of-balance flow can append discount/debit markers to RF$.
- Add Check: marks each invoice FLAG="P"
- Delete Check: rebuild LEDGER and CHECK temp files and swap; reset invoices back to FLAG="J"
- Sales Employee checks: reads EMPNAME.DAT; default commission 15%, “STEV*” 10%

---

## 5) AmiWinForms (VB.NET) — Current Implemented Wiring (from your scans + pasted code)
Main menu implemented and wired:
- (C) Checks and Cash Receipts -> opens FormLedgerMenu
- (?) About -> FormAbout
- (Z) Personal Calendar -> FormPersonalCalendar
- (1) Mileage Tracking -> FormMileageTracking
- (I) Backup Price List & Rolodex -> MigrationService + BackupService

CHECKS sub-menu (FormLedgerMenu.vb):
Wired:
- (3) View Checks -> FormLedgerView
- (5) View Company Totals -> FormLedgerCompanyTotals
- (8) View OTHER Checks -> FormOtherChecksView
- (9) Find a Check # -> FormFindByCheckNumber
- (0) Find an Invoice # -> FormFindByInvoiceNumber
Unwired/stubbed:
- (1) Add Checks
- (2) Delete Checks
- (6) Add OTHER Checks
- (7) Delete OTHER Checks
- (A) Find checks that don’t balance
- (S) Sales employee’s checks

Prompting differences vs DOS:
- VB.NET has 3 prompts (customer, check, year) and no month prompt.
- VB.NET prompts return raw text; blank is intended as ALL (matches your design intent).
- Current VB.NET code collects cust/chk/yr then opens FormLedgerView without applying them (so typed values are ignored).

FormLedgerView.vb:
- Filters by customer contains, check contains, year last-2-digits vs DateText.EndsWith(yy), month starts-with vs DateText.
- Loads LEDGER.CUR into _all; asynchronously loads CHECK.INV to map check->invoice list; reads invoice details from INVOICE.CHK.

---

## 6) Key Gaps Identified (DOS vs VB.NET)
- Implement Add Check workflow with DOS rules (file writes to LEDGER.CUR + CHECK.INV + invoice flag updates in INVOICE.CHK).
- Implement Delete Check workflow with DOS atomic rewrite semantics and invoice flag rollback.
- Implement OTHER.CHK add/delete (including DOS date validation rules).
- Implement Sales Employee checks report (EMPNAME.DAT parsing, commission logic).
- Optional: implement month prompt (DOS only asks month if year given).
- Decide what to do with (A) Doesn’t balance: DOS prints it but does nothing; VB.NET can implement a useful version.
