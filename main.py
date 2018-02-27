#python code for twilio response
import requests
from twilio.rest import Client
from flask import Flask, request, redirect
from twilio.twiml.messaging_response import MessagingResponse

account_sid = "*****"
auth_token = "*****"
client = Client(account_sid, auth_token)

rawData = requests.get()
userData = requests.get()

exists = False
alreadyCheckedIn = False

txtFile = open('schedule.txt')

for i in userData:
    if i['person_id'] == rawData['person_id']:
        exists = True
        phoneNum = rawData['phone']
        #Hi there first name
    else:
        exists = False
        #Create an account

if exists:
    if rawData['checkIn'] == 'true':
        alreadyCheckedIn = True
    else:
        for i in userData:
            if i['person_id'] == rawData['person_id']:
                userData['checkIn'] = "true"

if (exists == True) and (alreadyCheckedIn == False):
    outputTxt = "You've successfully checked in!"
    schedule = testFile.read()
elif alreadyCheckedIn:
    outputTxt = "You have already checked in! Would you like the schedule again? (Y/N)"
    if request.form['body'] == "Y":
        schedule = testFile.read()
    else:
        schedule = ''
        pass
else:
    outputTxt = "You've Successfully created an account and checked in!"
    schedule = testFile.read()

client.messages.create(
    to=phoneNum,
    from_="+19093231924",
    body= outputTxt + '\n' + schedule,
)
