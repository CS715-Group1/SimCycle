import requests

# Define the base URL of your Flask server
base_url = 'http://localhost:5454'

# Send a POST request to start the SUMO simulation
start_simulation_response = requests.post(f'{base_url}/sim/start')
if start_simulation_response.status_code == 200:

    response = start_simulation_response.json()
    print(response)
else:
    print(f"Failed to start SUMO simulation. Status code: {start_simulation_response.status_code}")

# Send a GET request to retrieve vehicle count
vehicle_count_response = requests.get(f'{base_url}/vehicle/count')
if vehicle_count_response.status_code == 200:
    vehicle_count = vehicle_count_response.json().get('vehicle_count', 0)
    print(f"Vehicle count in SUMO simulation: {vehicle_count}")
else:
    print(f"Failed to retrieve vehicle count. Status code: {vehicle_count_response.status_code}")
