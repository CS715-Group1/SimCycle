from flask import Blueprint, jsonify
import traci

vehicle_bp = Blueprint('vehicle', __name__)

@vehicle_bp.route('/count', methods=['GET'])
def count():
    # Get vehicle count from SUMO and return it
    vehicle_count = len(traci.vehicle.getIDList())
    return jsonify(vehicle_count=vehicle_count)