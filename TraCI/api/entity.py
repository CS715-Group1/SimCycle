from flask import request
import traci

@app.route("/pos", methods = ["GET"])
def pos():
  return "TEMP POS", 200

# Set 
@app.route("/pos", methods = ["PUT"])
def pos():
  return "TEMP POS", 200