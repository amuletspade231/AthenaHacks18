import json
def make_account():

    newObj = {
        'person_id' : '321',
        'f_name' : 'jane',
        'l_name' : 'doe',
        'email' : 'jane.doe@ucr.edu',
        'phone' : '9097654321',
        'checkIn' : 'true'
    }

    stf = json.dumps(newObj)
    f = open("sample.json", "a")
    f.write(stf)
    f.close()

make_account()