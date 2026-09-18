#!/bin/bash
# Simülatörün app data dizininden startup.log dosyasını bulup yazdırır.
SIM="EA44ADBF-FF8F-4D9A-8ABE-25277AD88C70"
LOG=$(find ~/Library/Developer/CoreSimulator/Devices/$SIM/data/Containers/Data/Application \
      -name "startup.log" 2>/dev/null | head -1)

if [ -z "$LOG" ]; then
  echo "startup.log bulunamadı. Uygulama hiç başlamadı (native crash)."
else
  echo "=== $LOG ==="
  cat "$LOG"
fi
