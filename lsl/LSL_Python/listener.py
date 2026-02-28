import csv
import sys
from pylsl import StreamInlet, resolve_byprop

print("Waiting for StealthGame_Events stream...")

streams = resolve_byprop("name", "StealthGame_Events", timeout=10)

if not streams:
    print("ERROR: LSL stream not found")
    sys.exit(1)

inlet = StreamInlet(streams[0])
print("Connected to LSL stream")
print("Press Ctrl+C or close the window to exit")

try:
    with open("game_logs.csv", "a", newline="") as f:
        writer = csv.writer(f)
        writer.writerow(["lsl_timestamp", "payload"])
        f.flush()

        while True:
            sample, timestamp = inlet.pull_sample(timeout=0.5)

            if sample is not None:
                writer.writerow([timestamp, sample[0]])
                f.flush()
                print("LOGGED:", sample[0])

except KeyboardInterrupt:
    print("\nExiting cleanly...")

finally:
    inlet.close_stream()
    print("LSL inlet closed")
