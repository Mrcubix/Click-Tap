cleanup_macos_build() {
  if [ -d "${OUTPUT}" ]; then
    if [ -d "${OUTPUT}/ClickTap.app" ]; then
      echo "Cleaning old build outputs..."
      rm -rf "${OUTPUT}/ClickTap.app"
    fi
  fi
}