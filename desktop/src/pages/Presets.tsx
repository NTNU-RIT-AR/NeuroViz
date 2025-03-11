// Load files from the folder

import { useEffect, useState } from "react";
import { invoke } from '@tauri-apps/api/core';

async function fetchFiles(): Promise<string[]> {
  try {
    return await invoke('list_presets');
  } catch (e) {
    console.log("could not fetch files: ", e)
    return Promise.resolve([]);
  }
}

// Delete a file
// async function deleteFile(fileName: string) {
//   await invoke('delete_file')
// }

export default function PresetsPage() {
  const [files, setFiles] = useState<string[]>([]);

  useEffect(() => {
    fetchFiles().then(setFiles);
  }, [])

  return (
    <>
      <p>presets page text</p>
      <ul>
        {files.map((file) => (
          <li key={file}>{file}</li>
        ))}
      </ul>
    </>

  )
}
