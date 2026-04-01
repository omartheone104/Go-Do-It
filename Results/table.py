import pandas as pd
import plotly.graph_objects as go

df = pd.read_csv("./data.csv")

fig = go.Figure(
    data=[
        go.Table(
            header=dict(
                values=list(df.columns),
                fill_color="lightblue",
                align="center",
                font=dict(color="black", size=14)
            ),
            cells=dict(
                values=[df[col] for col in df.columns],
                align="center",
                font=dict(color="black", size=12)
            )
        )
    ]
)

# Show the table in a browser or notebook
fig.show()
fig.write_image("./results_table.png")
