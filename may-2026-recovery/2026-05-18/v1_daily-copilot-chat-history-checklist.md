# Daily Copilot Chat History Checklist

---

## Beginning of Day

### Save and Prepare Files
- [ ] Export or save the previous chat session
- [ ] Save the files in a reviewable format:
  - [ ] `.json`
  - [ ] `.md`
  - [ ] `.txt`
- [ ] Avoid using `.zip` unless absolutely necessary
- [ ] Create a dated local folder for today's files
  - [ ] Example: `E:\AMConversionSaves\2026-05-18\`

### If Files Are Zipped
- [ ] Extract the ZIP file before uploading
- [ ] Confirm the extracted files are reviewable
- [ ] Remove or ignore oversized archive files

### Update Local Repo
- [ ] Open PowerShell
- [ ] Go to the local chat history repo
  - [ ] `cd "E:\Co-Pilot-Chat-History"`
- [ ] Switch to the working branch
  - [ ] `git checkout upload-chat-history`
- [ ] Pull latest changes
  - [ ] `git pull origin upload-chat-history`

### Copy New Files Into Repo
- [ ] Create today’s folder in the repo
  - [ ] Example: `E:\Co-Pilot-Chat-History\may-2026-recovery\2026-05-18\`
- [ ] Copy today’s saved files into the repo folder
- [ ] Confirm the files are present in the repo folder

### Commit and Push
- [ ] Run `git status`
- [ ] Add today’s folder
  - [ ] `git add "may-2026-recovery\2026-05-18"`
- [ ] Run `git status` again and confirm files are staged
- [ ] Commit the files
  - [ ] `git commit -m "Add chat history files for 2026-05-18"`
- [ ] Push to GitHub
  - [ ] `git push origin upload-chat-history`

### Start the New Chat
- [ ] Tell Copilot which repo to review
  - [ ] Repo: `testsubject117/Co-Pilot-Chat-History`
- [ ] Tell Copilot which branch to review
  - [ ] Branch: `upload-chat-history`
- [ ] Tell Copilot which folder or file to review
- [ ] Ask for a summary of where work left off

Example prompt:
> Review `may-2026-recovery/2026-05-18/` in `testsubject117/Co-Pilot-Chat-History` on branch `upload-chat-history` and summarize where we left off.

---

## End of Day

### Create a Handoff Summary
- [ ] Ask Copilot for an end-of-day summary
- [ ] Save the summary locally
- [ ] Save any session export files locally
- [ ] Save open items / next steps locally

Suggested files:
- [ ] `2026-05-18-end-of-day-summary.md`
- [ ] `2026-05-18-open-items.md`
- [ ] `2026-05-18-session-export.json`

### Confirm the Handoff Includes
- [ ] Completed work
- [ ] Work still in progress
- [ ] Blockers
- [ ] Exact next step
- [ ] Important file paths
- [ ] Important branches
- [ ] Important commands still needing to be run

### Copy End-of-Day Files Into Repo
- [ ] Copy summary and session files into the correct dated repo folder
- [ ] Confirm the files are present

### Commit and Push End-of-Day Update
- [ ] Run `git status`
- [ ] Add the updated dated folder
  - [ ] `git add "may-2026-recovery\2026-05-18"`
- [ ] Commit the end-of-day files
  - [ ] `git commit -m "Add end of day notes for 2026-05-18"`
- [ ] Push to GitHub
  - [ ] `git push origin upload-chat-history`

### Optional but Recommended
- [ ] Update `may-2026-recovery\current-status.md`
- [ ] Confirm `current-status.md` includes:
  - [ ] Current objective
  - [ ] Latest known good state
  - [ ] Immediate next action
  - [ ] Current blockers
  - [ ] Important file paths
  - [ ] Important branches

---

## Quick Morning Version
- [ ] Save yesterday’s chat
- [ ] Put files in a dated folder
- [ ] Copy files into repo
- [ ] Run:
  - [ ] `git checkout upload-chat-history`
  - [ ] `git pull origin upload-chat-history`
  - [ ] `git add "may-2026-recovery\YYYY-MM-DD"`
  - [ ] `git commit -m "Add chat history for YYYY-MM-DD"`
  - [ ] `git push origin upload-chat-history`
- [ ] Tell Copilot what folder to review

---

## Quick End-of-Day Version
- [ ] Ask Copilot for summary and open items
- [ ] Save files locally
- [ ] Copy them into dated repo folder
- [ ] Run:
  - [ ] `git add "may-2026-recovery\YYYY-MM-DD"`
  - [ ] `git commit -m "Add end of day notes for YYYY-MM-DD"`
  - [ ] `git push origin upload-chat-history`

---

## Important Reminders
- [ ] Do upload extracted `.json`, `.md`, `.txt` files
- [ ] Do use dated folders
- [ ] Do include a summary file
- [ ] Do tell Copilot the exact branch and path
- [ ] Do keep `current-status.md` updated
- [ ] Do not upload files over 100 MB to GitHub
- [ ] Do not rely on memory alone without uploaded files
- [ ] Do not leave out the exact next step
- [ ] Do not mix unrelated project notes unless clearly labeled