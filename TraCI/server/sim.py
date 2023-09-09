from flask import Blueprint, jsonify
import uuid
import SessionStore

sim_bp = Blueprint('sim', __name__)


FolderPath = "../SUMO_Networks/"
SumoNetwork = "Rectangle/Network_01.sumocfg"


# Start the SUMO simulation
@sim_bp.route('/start', methods=['POST'])
def start():
    # traci.start(["sumo-gui", "-c", FolderPath + SumoNetwork, "--start"])
    sim = SessionStore.Session(FolderPath + SumoNetwork)
    id = uuid.uuid1().hex
    SessionStore.sessions.update({id: sim})

    return jsonify({"id": id, "message": "SUMO simulation started"})

@sim_bp.route('/step/<id>', methods=['POST'])
def step(id):
    if id not in SessionStore.sessions:
        return jsonify(message="Cannot find session with id: " + id)
    
    session = SessionStore.sessions.get(id)
    session.step()
    return jsonify(message="Step simulation")

@sim_bp.route('/stop/<id>', methods=['POST'])
def stop(id):
    if id not in SessionStore.sessions:
        return jsonify(message="Cannot find session with id: " + id)
    
    session = SessionStore.sessions.get(id)
    session.stop()
    return jsonify(message="Stop simulation")


# # Stop the SUMO simulation
# @sim_bp.route('/stop/<str:id>', methods=['POST'])
# def stop(id):
#     if id in store.simulations:
#         store.simulations.get(id)
#     traci.close()
#     return jsonify(message="SUMO simulation stopped")