# AMi WinForms Port Status & Recovery Roadmap

## Summary

This project is a **substantial but incomplete VB.NET / WinForms port** of the historical DOS / GW-BASIC AMI workflows.

The codebase is **well beyond prototype stage** and already implements a meaningful amount of **read-side legacy functionality**, including ledger/check viewing, company totals, invoice/check searching, mileage tracking, personal calendar, backup support, and legacy-format readers.

However, the port is **not yet feature-complete** as a DOS replacement. The largest remaining gap is the **write-side accounting workflow**, especially **Add Check**, related PRC/company validation, and associated legacy file updates.

Based on:
- repo/code comparison results,
- recovered chat-history summaries,
- and visible unfinished integration points,

the most likely interrupted work area before the crash was **Sales Employee checks integration**, with the broader intended milestone being a DOS-faithful **Add Check** workflow.

---

## Current status

### Done

#### Core app structure
- VB.NET / WinForms application exists
- Main menu shell exists
- DOS-style menu organization is substantially represented
- Centralized path helpers exist
- Migration service exists
- Backup service exists

#### Read-side checks/accounting
- Checks submenu exists
- View Checks implemented
- View Company Totals implemented
- View OTHER Checks implemented
- Find by Check Number implemented
- Find by Invoice Number implemented
- Doesn’t Balance report implemented

#### Legacy file reading/parsing
- `LEDGER.CUR` reader implemented
- `CHECK.INV` reader implemented
- `INVOICE.CHK` reader implemented
- `OTHER.CHK` reader implemented
- DOS EOF handling represented
- GW-BASIC-style text/quote parsing represented
- MBF decoding for legacy numeric records implemented
- Defensive file error handling exists

#### Other implemented modules
- Mileage Tracking substantially implemented
- Personal Calendar substantially implemented
- `EMPNAME.DAT` editor exists

#### Recovery/context
- Chat-history archive repo identified
- May 2026 recovery files uploaded to GitHub
- Pre-crash summary recovered
- Repo comparison scan completed

---

### Partial

#### DOS parity
- DOS menu wording/structure preserved in many places
- DOS behavior parity is only partial, not complete
- Some prompt/filter semantics are approximated rather than exact

#### Sales Employee checks
- Sales Employee submenu exists
- Sales Employee wizard exists
- Wizard loads `EMPNAME.DAT`
- Wizard reads `INVOICE.CHK`
- DOS commission logic is represented
- Wizard generates a DOS-style report
- Wizard is not fully integrated into user flow
- Menu option **B** does not yet launch the wizard properly
- Workflow appears report-oriented, not full transactional parity

#### PRC/company handling
- PRC files are preserved in migration/backup logic
- PRC files are not yet actively used in live company validation flow
- No real company-file selection workflow is present

#### View Checks parity
- View Checks exists and supports filtering
- Prompt values are not passed through exactly like DOS
- Month/year/customer semantics are only partially DOS-faithful

#### Modernization status
- App is beyond prototype stage
- Several file-backed modules are genuinely usable
- App is not yet a complete DOS replacement

---

### Not started / still missing

#### Critical write-side accounting workflows
- Add Check
- Delete Check
- Add OTHER Check
- Delete OTHER Check

#### Legacy write operations
- Append to `LEDGER.CUR`
- Append to `CHECK.INV`
- Update `INVOICE.CHK` payment/status flags
- Delete/rebuild rollback flow for check deletion
- Full DOS balancing/debit/discount write logic

#### PRC operational parity
- Validate company/customer using `PRC\<CODE>.PRC` in live workflow
- Build company validation into Add Check path
- Implement full company-file operational parity

#### End-to-end accounting parity
- Full Sales Employee check creation flow
- Check printing/output parity
- Full transactional checks workflow
- End-to-end DOS-faithful write-side verification

#### Broader feature audit
- Confirm remaining DOS modules outside the reviewed areas
- Confirm Rolodex parity
- Confirm price-list parity
- Confirm other unimplemented main-menu branches

#### Recovery/documentation follow-through
- Extract explicit next-step instructions from May 4–6 chats
- Convert findings into a persistent roadmap document
- Mark each major DOS workflow with status and parity notes

---

## Most likely interrupted task

The clearest visible “stopped mid-task” area is **Sales Employee checks**.

Evidence:
- `FormSalesEmployeeCheckWizard` exists and contains substantial logic.
- It loads `EMPNAME.DAT`.
- It reads `INVOICE.CHK`.
- It applies DOS commission rules.
- It generates a DOS-style report.
- But it is not properly launched from the submenu path that appears intended to use it.

This strongly suggests the last interrupted implementation pass was likely:
1. build Sales Employee wizard/report flow,
2. wire it into the CHECKS submenu,
3. validate against real legacy data.

---

## Most likely broader intended milestone

The May 6 recovered summary indicates work had also moved into the broader **DOS-faithful Add Checks** area, including:
- PRC company validation against the network share,
- core legacy file writers,
- and a shift away from at least one wizard-style approach.

That makes **Add Check** the most likely next major milestone after the immediate Sales Employee integration issue is resolved.

---

## Priority roadmap

### Now
1. **Finish Sales Employee checks integration**
   - Wire menu option **B** in `FormSalesEmployeesChecksMenu`
   - Launch `FormSalesEmployeeCheckWizard`
   - Verify it works end-to-end with real `EMPNAME.DAT` and `INVOICE.CHK`
   - Confirm whether it is intended to remain report-only for now or evolve into actual check creation

2. **Mine May 4–6 recovery chats for exact next-step intent**
   - Identify what was explicitly planned next before the crash
   - Confirm whether the wizard was a temporary branch or the main intended flow
   - Capture notes about PRC/company validation and file writers

3. **Keep this status document updated**
   - Use it as the single current roadmap/status source

---

### Next
1. **Implement Add Check**
   - Validate company/customer using PRC files
   - Write to `LEDGER.CUR`
   - Append to `CHECK.INV`
   - Update `INVOICE.CHK` status/flags
   - Preserve DOS balancing/debit/discount behavior

2. **Decide and standardize company/PRC workflow**
   - Confirm whether validation is silent/file-based or user-driven
   - Determine whether a company picker UI is needed
   - Align with DOS semantics rather than ad hoc modern behavior

3. **Improve DOS prompt parity for View Checks**
   - Pass prompt values through correctly
   - Support blank/all semantics exactly
   - Align customer/year/month handling with DOS expectations

4. **Define transaction safety for legacy writes**
   - Atomic rewrite vs append strategy
   - Backup-before-write strategy
   - Recovery behavior on write failure

---

### Later
1. **Implement Delete Check**
   - Rebuild/rollback logic
   - Preserve DOS deletion semantics

2. **Implement Add/Delete OTHER checks**
   - Complete write-side parity for `OTHER.CHK`

3. **Audit remaining DOS modules**
   - Rolodex
   - Price list
   - Other main-menu branches not yet verified

4. **Polish parity and UX**
   - Exact DOS prompt behavior
   - Output/report fidelity
   - Handling of legacy edge cases

5. **Create a formal parity matrix**
   - DOS module
   - WinForms equivalent
   - Status
   - Missing behaviors
   - File dependencies

---

## Practical working conclusion

The VB.NET port is:
- **real**
- **substantial**
- **already useful in several areas**
- but **not yet complete**

The current best working interpretation is:

- **Immediate interrupted task:** finish wiring and validating Sales Employee checks flow
- **Next major milestone:** implement the DOS-faithful Add Check write path
- **Largest remaining gap:** write-side accounting parity

---

## Recovery sources used

Primary recovered context came from:
- `testsubject117/Co-Pilot-Chat-History`
- especially the `may-2026-recovery` folder and the May 4–6, 2026 chat exports

These recovery artifacts should continue to be treated as the best source for reconstructing pre-crash intent until the remaining chats are fully mined.