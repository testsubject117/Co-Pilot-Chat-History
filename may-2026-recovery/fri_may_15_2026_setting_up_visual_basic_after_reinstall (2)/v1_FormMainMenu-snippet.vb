Dim svc As New CompanyLookupService("\\invoice\MainMenu\Data\REALNAME.DAT")
svc.LoadData()

Dim company As CompanyInfo = svc.FindExact("3VFASTEN")

If company IsNot Nothing Then
    MessageBox.Show("Found: " & company.CompanyCode & " - " & company.CompanyName)
Else
    MessageBox.Show("Company not found.")
End If