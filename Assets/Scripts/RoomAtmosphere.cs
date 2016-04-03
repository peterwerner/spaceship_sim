using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomAtmosphere {

	public static void UpdateParticles(Room room, float timeStep)
	{
		foreach (RoomConnector connector in room.Connectors) {
			// Handle in-flowing connections
			if (connector.GetOutflowRate(room) < 0) 
			{
				ParticleSystem.Particle[] p = new ParticleSystem.Particle[connector.Particles.particleCount + 1];
				int k = connector.Particles.GetParticles(p);
				float magnitude = -1 * connector.GetOutflowRate(room);
				Vector3 destination = room.transform.position;
				for (int i = 0; i < k; i++) 
				{
					// If there are outflowing connections, choose one as destination
					RoomConnector nearest = null;
					float costNearest = Mathf.Infinity;
					foreach (RoomConnector connectorOut in room.Connectors) {
						float flow = connectorOut.GetOutflowRate(room);
						float cost = flow * Vector3.Distance(p[i].position, connectorOut.transform.position);
						if (flow > 0 && cost < costNearest) {
							nearest = connectorOut;
							costNearest = cost;
						}
					}
					if (nearest)
						destination = nearest.transform.position;
					Vector3 direction = destination - connector.transform.position;
					p[i].velocity = Vector3.Lerp(p[i].velocity, magnitude * (direction).normalized, timeStep);
				}
				connector.Particles.SetParticles(p, k);
			}
			// Handle out-flowing connections where the other room is null
			else if (connector.GetOutflowRate(room) > 0 && connector.GetOtherRoom(room) == null) {
				ParticleSystem.Particle[] p = new ParticleSystem.Particle[connector.Particles.particleCount + 1];
				int k = connector.Particles.GetParticles(p);
				float magnitude = connector.GetOutflowRate(room);
				Vector3 direction = connector.transform.position - room.transform.position;
				for (int i = 0; i < k; i++) 
					p[i].velocity = Vector3.Lerp(p[i].velocity, magnitude * (direction).normalized, timeStep);
				connector.Particles.SetParticles(p, k);
			}
		}
	}

}
