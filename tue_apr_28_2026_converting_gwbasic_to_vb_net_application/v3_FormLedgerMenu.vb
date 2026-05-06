Private Shared Sub ConfigureCloseButton(btn As Button, text As String)
    btn.Text = text

    ' Match the "small grey Close" style used in the code-built forms
    btn.AutoSize = True
    btn.Height = 32
    btn.Margin = New Padding(3, 14, 3, 3)
    btn.Anchor = AnchorStyles.Right

    btn.TextAlign = ContentAlignment.MiddleCenter
    btn.Font = New Font("Segoe UI", 10.0F, FontStyle.Regular, GraphicsUnit.Point)

    ' Grey background, black text (standard button rendering)
    btn.UseVisualStyleBackColor = True
    btn.BackColor = SystemColors.Control
    btn.ForeColor = Color.Black
    btn.FlatStyle = FlatStyle.Standard
End Sub