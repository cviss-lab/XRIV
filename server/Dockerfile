# setup environment
FROM python:3.8
WORKDIR /app
ADD . /app

# nvidia-docker hooks
LABEL com.nvidia.volumes.needed="nvidia_driver"
ENV PATH /usr/local/nvidia/bin:${PATH}
ENV LD_LIBRARY_PATH /usr/local/nvidia/lib:/usr/local/nvidia/lib64:${LD_LIBRARY_PATH}

# install requirements for opencv
RUN apt-get update -y
RUN apt install libgl1-mesa-glx -y

# install requirements
RUN pip install -r requirements.txt

# expose port
EXPOSE 80

# run entrypoint script
CMD python server.py