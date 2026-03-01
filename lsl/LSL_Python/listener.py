import csv
import sys
from datetime import datetime
import time
from pylsl import StreamInlet, resolve_byprop, resolve_streams

print("Waiting for StealthGame_Events stream...")

# Keep retrying until a stream is found or user interrupts
try:
    while True:
        streams = resolve_byprop("name", "StealthGame_Events", timeout=5)
        if streams:
            break

        # For diagnostics, list discovered streams briefly
        found = resolve_streams()
        names = [s.name() for s in found]
        print(f"No matching stream yet. Known streams: {names}. Retrying...")
        time.sleep(1)

    inlet = StreamInlet(streams[0])
    print("Connected to LSL stream")
    print("Press Ctrl+C or close the window to exit")
except KeyboardInterrupt:
    print("Interrupted while waiting for stream. Exiting.")
    sys.exit(1)

try:
    timestamp_str = datetime.now().strftime("%Y-%m-%d_%H-%M-%S")
    filename = f"game_logs_{timestamp_str}.csv"

    with open(filename, "w", newline="", encoding="utf-8") as f:
        writer = csv.writer(f)

        writer.writerow([
            "timestamp_unix",
            "scene_name",
            "user_id",
            "event_type",
            "context",
            "value"
        ])
        f.flush()

        while True:
            sample, timestamp = inlet.pull_sample(timeout=0.5)

            if not sample:
                continue

            payload = sample[0]
            parts = payload.split("|")

            scene_name = parts[0] if len(parts) > 0 else ""
            user_id = parts[1] if len(parts) > 1 else ""
            event_type = parts[2] if len(parts) > 2 else ""
            context = parts[3] if len(parts) > 3 else ""
            value = parts[4] if len(parts) > 4 else ""

            writer.writerow([
                timestamp,
                scene_name,
                user_id,
                event_type,
                context,
                value
            ])
            f.flush()

            print("LOGGED:", timestamp, scene_name, user_id, event_type, context, value)

except KeyboardInterrupt:
    print("\nExiting cleanly...")

finally:
    inlet.close_stream()
    print("LSL inlet closed")
