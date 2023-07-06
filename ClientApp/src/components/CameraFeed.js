import React from 'react';

export class CameraFeed extends React.Component {
    constructor(props) {
        super(props);
        this.state = { imgSrc: '' };
        this.interval = null;
    }

    componentDidMount() {
        this.interval = setInterval(() => {
            fetch('camera/stream')
                .then(response => response.blob())
                .then(blob => {
                    const imageSrc = URL.createObjectURL(blob);
                    this.setState({ imgSrc: imageSrc });
                });
        }, 1000/60.0); // 60fps
    }

    componentWillUnmount() {
        clearInterval(this.interval);
    }

    render() {
        return (
            <div>
                <h1>Camera Feed</h1>
                <h2>stream: {this.state.imgSrc}</h2>
                <img src={this.state.imgSrc} alt="camera feed" />
                <h2>stream2</h2>
                <img src="camera/stream2" alt="camera feed" />
            </div>
        );
    }
}

