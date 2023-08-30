from flask import request
import traci

@app.route("/start", methods = ["POST"])
def start():
  # Connect to SUMO simulation
  traci.start(["sumo", "-c", "path/to/your/sumocfg/file.sumocfg"])

  return "Started SUMO simulation.", 200

@app.route("/stop", methods = ["POST"])
def stop():
  # Close TraCI connection
  traci.close()
  return "Stopped SUMO simulation.", 200

@app.route("/step", methods = ["POST"])
def step():
  traci.simulationStep()
  
  return "Advanced one step.", 200