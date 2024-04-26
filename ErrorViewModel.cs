curl.exe -X POST https://api.twilio.com/2010-04-01/Accounts/AC730ea004ff6c24/Calls.json ^
  --data-urlencode "Url=http://demo.twilio.com/docs/voice.xml" ^
  --data-urlencode "To=+918319338" ^
  --data-urlencode "From=+12512011" ^
  -u "A:"
