START ../../PacketGenerator/bin/PacketGenerator.exe ../../PacketGenerator/PDL.xml
XCOPY /Y GenPackets.cs "../../DummyClient/Packet/GenPackets.cs"
XCOPY /Y GenPackets.cs "../../Server/Packet/GenPackets.cs"

XCOPY /Y ClientPacketManager.cs "../../DummyClient/Packet/ClientPacketManager.cs"
XCOPY /Y ServerPacketManager.cs "../../Server/Packet/ServerPacketManager.cs"

