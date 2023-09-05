import time
import TrafficSimulator

sessions = {}

class Session(object):
    def __init__(self, SumoNetwork):

        self.NetworkName = SumoNetwork

        #Launch SUMO
        self.TrafficSim = TrafficSimulator.TrafficSimulator(self.NetworkName)
        self.TrafficLights = self.TrafficSim.ParseTrafficLights()
        self.SumoObjects = []
    
    def step(self):
        #Update SUMO
        self.SumoObjects, self.TrafficLights = self.TrafficSim.StepSumo(self.SumoObjects, self.TrafficLights)

    # def main(self):

    #     deltaT = 0.02

    #     while True:

    #         #Get timestamp
    #         TiStamp1 = time.time()
            
    #         #Update SUMO
    #         self.SumoObjects, self.TrafficLights = self.TrafficSim.StepSumo(self.SumoObjects, self.TrafficLights)

    #         #Synchronize time
    #         TiStamp2 = time.time() - TiStamp1
    #         if TiStamp2 > deltaT:
    #             pass
    #         else:
    #             time.sleep(deltaT-TiStamp2)
