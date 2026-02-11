## Unity Setup
- For each agent in the scene, make sure the behavior parameter component has no model set
- Also, behaviour type should be `Default`


## Training

Step 0: Open terminal in vscode

Step 1: Activate venv in terminal
```bash
pyenv activate venv
```

Step 2: Run new training
```bash
mlagents-learn config/config.yaml --run-id="RUN_NAME_HERE"
```

Resume training
```bash
mlagents-learn config/config.yaml --run-id="RUN_NAME_HERE" --resume
```

Overwrite training
```bash
mlagents-learn config/config.yaml --run-id="RUN_NAME_HERE" --force
```