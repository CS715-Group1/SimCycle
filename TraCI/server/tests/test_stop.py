import requests
import argparse

parser = argparse.ArgumentParser()

# Define the argument(s) you want to accept
parser.add_argument('id', type=str)

args = parser.parse_args()
session_id = args.id



# Define the base URL of your Flask server
base_url = 'http://localhost:5454'

# Send a POST request to stop the SUMO simulation
response = requests.post(f'{base_url}/sim/stop/{session_id}').json()

print(response)