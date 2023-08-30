import traci

config = "../Sumo/Saves/tutorial1.sumocfg"

sumoBinary = "sumo-gui"  # sumo or sumo-gui
sumoCmd = [sumoBinary, "-c", config, "--log", "logfile.txt"]

traci.start(sumoCmd)
step = 0
while step < 100:
    traci.simulationStep()
    x, y = traci.vehicle.getPosition("t_0")
    a = traci.vehicle.getAcceleration("t_0")
    traci.vehicle.slowDown("t_0", 1, 2)
    print(f"Vehicle position: x={x}, y={y}, a={a}")

    step += 1


traci.close()