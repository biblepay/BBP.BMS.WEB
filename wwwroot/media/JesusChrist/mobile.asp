
<!DOCTYPE HTML>
<html lang="en-US">
<head>
        <script src='commonframe.js'></script>
            <title>Jesus-Christ.US</title>

               <link href='Styles/site_daylight.css' rel='stylesheet' type='text/css' />



</head>
<style>

.grad10
{
  xbackground: #466368;
  background: -webkit-linear-gradient(left,white,darkblue);
  background:    -moz-linear-gradient(left,white,darkblue);
  background:         linear-gradient(left,white,darkblue);
  color:yellow;
  background-image: url("images/clouds.jpg");
	opacity:.66;
}


.cent10
{
    text-align: center;
}



.body1
{
    margin-top: 25px;
    margin-bottom: 25px;
    margin-right: 125px;
    margin-left: 125px;
    font-size: 111%;
}

</style>

<body class='body1'>


<%



    Function ExtractXML(sData, sStartKey, sEndKey, sSecondEndKey,bSecondElement)
        if len(sData) = 0 then ExtractXML="":Exit function

        Dim iPos1
        iPos1 = InStr(1, sData, sStartKey)
        iPos1 = iPos1 + Len(sStartKey)

        iPos2 = InStr(iPos1, sData, sEndKey)

        if bSecondElement then
        	iPos1 = iPos2+1
        	iPos2 = instr(iPos1,sData,sSecondEndKey)

        end if

        If iPos2 = 0 Then ExtractXML="":Exit Function


        Dim sOut
        sOut = Mid(sData, iPos1, iPos2 - iPos1)
        ExtractXML=sOut
    End Function



	set fs=Server.CreateObject("Scripting.FileSystemObject")
	Set f=fs.OpenTextFile(Server.MapPath("main.htm"), 1)
	s = "	<h1>      <div class='cent10 grad10'>      <span>JESUS-CHRIST.US</span></h1><TABLE><font face=arial>"
	bAlt = false

	Do Until f.AtEndOfStream

	   sTemp = f.ReadLine
	   sURL = "" & ExtractXML(sTemp,"href=",">","",false)
	   sComment = "" & ExtractXML(sTemp,"title='","'","",false)
	   sTitle = ExtractXML(sTemp,"href=",">","</a>",true)
       if instr(1,sURL,"<script") > 0 then sURL=""
       if instr(1,sURL,"Styles/") > 0 then sURL=""
       if instr(1,sComment,"y style=") > 0 then sComment=""
       if instr(1,sComment,"div id") > 0 then sComment = ""
	   if instr(1,sComment,"<table width=") > 0 then sComment=""
	   if instr(1,sComment,"<div class=") > 0 then sComment = ""
	   if instr(1,sComment,"table class=") > 0 then sComment=""
	   if instr(1,sComment,"<SPAN>") > 0 then sComment = ""
	   if instr(1,sComment,"class=") > 0 then sComment=""
	   if instr(1,sComment,"<li><a target=Jesus") > 0 then sComment=""
       if instr(1,sComment,"style=") > 0 then sComment = ""
       if instr(1,sComment,"able width=") > 0 then sComment = ""
       if instr(1,sComment,"iv id=") > 0 then sComment = ""
if instr(1,sComment,"<iframe id=") > 0 then sComment=""

  	   if len(sURL) > 5 and len(sComment) > 5 then
	  		'Make section
	  		if bAlt then sColor = "lightgrey" else sColor="white"
	  		sStyle = "style='font-face:arial;font-size:25px;background-color:" + sColor + ";font-color=yellow;width=50%;margin=10%' "
	  		section = "<TR><TD " + sStyle + " width='66%'><A href=" + sURL + ">" + sTitle + "</A></TD></TR><TR><TD " + sStyle + "><SPAN>" + sComment + "</SPAN></TD></TR>" + vbcrlf
	  		s = s + section
            bAlt = not bAlt
	  end if
    Loop
    f.Close
    s = s + "</TABLE>"

	response.write(s)


%>


<form action="FreeBibles.asp" method="post">
</form>

</body>
</html>